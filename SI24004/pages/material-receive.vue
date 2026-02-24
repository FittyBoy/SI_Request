<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'

useHead({
  title: 'Material Withdrawal - LP Control System',
  meta: [
    { name: 'description', content: 'Material withdrawal system for LP manufacturing' }
  ]
})

// Type definitions
interface MateralInventory {
  id: string
  matName: string
  matQuantity: number
  matUnit?: string
  matTypeId: string
  case: string
  expDate: string | null
  empId: string
  shift: string
  product: string
  supplier: string
  lotNumber: string
  location: string
  // Display names
  shiftName?: string
  productName?: string
  supplierName?: string
}

interface WithdrawalRequest {
  shift: string
  product: string
  matName: string
  machine: string
  quantity: number
  cause: string
  employee: string
  materialId: string
}

interface Withdrawal {
  id: number
  timestamp: Date
  matName: string
  quantity: number
  machine: string
  cause: string
  employee: string
  employeeName: string
  shift: string
  shiftName: string
  product: string
  productName: string
}

// Reactive state
const formData = ref<WithdrawalRequest>({
  shift: '',
  product: '',
  matName: '',
  machine: 'LP603',
  quantity: 0,
  cause: 'Add In Loop',
  employee: '',
  materialId: ''
})

const materials = ref<MateralInventory[]>([])
const selectedMaterial = ref<MateralInventory | null>(null)
const loadingStock = ref<boolean>(false)
const loadingMaterials = ref<boolean>(false)
const isSubmitting = ref<boolean>(false)
const validationErrors = ref<string[]>([])
const successMessage = ref<string>('')
const recentWithdrawals = ref<Withdrawal[]>([])

// Dynamic options from API
const products = ref<any[]>([])
const shifts = ref<any[]>([])
const suppliers = ref<any[]>([])
const employees = ref<any[]>([])

// API Configuration
const config = useRuntimeConfig()
const API_BASE_URL = config.public.apiBase + '/api/SI25006'

// Static constants
const DEFAULT_MACHINES = ['LP603', 'LP604', 'LP605']
const DEFAULT_CAUSES = [
  'Add In Loop',
  'Gravity Low',
  'Cor New Plate',
  'Cor Plate FN Over',
  'Cor Plate Scratch',
  'Cor Start up',
  'Grinding Carrier',
  'Slurry Abnormal'
]

// Computed properties
const availableStock = computed((): number => {
  return selectedMaterial.value?.matQuantity || 0
})

// Methods
const loadMasterData = async (): Promise<void> => {
  try {
    // Load products
    const { data: productsData } = await useFetch<any[]>(`${API_BASE_URL}/products`, {
      server: false
    })
    if (productsData.value) products.value = productsData.value

    // Load shifts
    const { data: shiftsData } = await useFetch<any[]>(`${API_BASE_URL}/shifts`, {
      server: false
    })
    if (shiftsData.value) {
      shifts.value = shiftsData.value
      // Set default shift
      if (shifts.value.length > 0 && !formData.value.shift) {
        formData.value.shift = shifts.value[0].id
      }
    }

    // Load employees
    const { data: employeesData } = await useFetch<any[]>(`${API_BASE_URL}/employees`, {
      server: false
    })
    if (employeesData.value) employees.value = employeesData.value

  } catch (error) {
    console.error('Error loading master data:', error)
  }
}

const loadMaterials = async (): Promise<void> => {
  loadingMaterials.value = true
  try {
    const { data: materialsData, error } = await useFetch<MateralInventory[]>(API_BASE_URL, {
      server: false
    })

    if (!error.value && materialsData.value) {
      materials.value = materialsData.value.map(material => ({
        ...material,
        matUnit: material.matUnit || 'kg',
        matQuantity: material.matQuantity || 0
      }))

      // Set default material
      if (materials.value.length > 0 && !selectedMaterial.value) {
        const defaultMat = materials.value.find(m => m.matName === 'FO1500') || materials.value[0]
        selectedMaterial.value = defaultMat
        formData.value.matName = defaultMat.matName
        formData.value.materialId = defaultMat.id
      }
    }
  } catch (error) {
    console.error('Error loading materials:', error)
    showError('Failed to load materials from server')
  } finally {
    loadingMaterials.value = false
  }
}

const onMaterialChange = (): void => {
  const selected = materials.value.find(m => m.matName === formData.value.matName)
  selectedMaterial.value = selected || null
  formData.value.materialId = selected?.id || ''
  
  // Auto-fill related data from selected material
  if (selected) {
    if (selected.product && !formData.value.product) {
      formData.value.product = selected.product
    }
    if (selected.shift && !formData.value.shift) {
      formData.value.shift = selected.shift
    }
  }
  
  clearMessages()
}

const checkStock = async (): Promise<void> => {
  if (!selectedMaterial.value) return

  loadingStock.value = true
  try {
    const { data: materialData, error } = await useFetch<MateralInventory>(
      `${API_BASE_URL}/${selectedMaterial.value.id}`, 
      { server: false }
    )

    if (!error.value && materialData.value) {
      const materialIndex = materials.value.findIndex(m => m.id === selectedMaterial.value?.id)
      if (materialIndex !== -1) {
        materials.value[materialIndex] = {
          ...materialData.value,
          matUnit: materialData.value.matUnit || 'kg',
          matQuantity: materialData.value.matQuantity || 0
        }
        selectedMaterial.value = materials.value[materialIndex]
      }
      showSuccess('Stock data refreshed successfully')
    }
  } catch (error) {
    console.error('Error checking stock:', error)
    showError('Failed to refresh stock data')
  } finally {
    loadingStock.value = false
  }
}

const validateForm = (): boolean => {
  validationErrors.value = []

  if (!formData.value.shift) validationErrors.value.push('Please select a shift')
  if (!formData.value.product) validationErrors.value.push('Please select a product')
  if (!formData.value.matName) validationErrors.value.push('Please select a material')
  if (!formData.value.machine) validationErrors.value.push('Please select a machine')
  if (!formData.value.cause) validationErrors.value.push('Please select a cause')
  if (!formData.value.employee) validationErrors.value.push('Please select an employee')
  if (!formData.value.quantity || formData.value.quantity <= 0) {
    validationErrors.value.push('Please enter a valid quantity')
  }

  if (selectedMaterial.value && formData.value.quantity > (selectedMaterial.value.matQuantity || 0)) {
    validationErrors.value.push(
      `Insufficient stock. Available: ${selectedMaterial.value.matQuantity} ${selectedMaterial.value.matUnit}`
    )
  }

  return validationErrors.value.length === 0
}

const handleWithdrawal = async (): Promise<void> => {
  clearMessages()

  if (!validateForm() || !selectedMaterial.value) {
    if (!selectedMaterial.value) {
      validationErrors.value.push('No material selected')
    }
    return
  }

  isSubmitting.value = true

  try {
    const currentQuantity = selectedMaterial.value.matQuantity || 0
    const newQuantity = currentQuantity - formData.value.quantity

    if (newQuantity < 0) {
      throw new Error('Insufficient stock for withdrawal')
    }

    // Update material quantity via API
    const { error } = await useFetch(`${API_BASE_URL}/${selectedMaterial.value.id}/quantity`, {
      method: 'PATCH',
      body: newQuantity,
      server: false
    })

    if (error.value) {
      throw new Error(error.value.message || 'Failed to update material quantity')
    }

    // Update local state
    selectedMaterial.value.matQuantity = newQuantity
    const materialIndex = materials.value.findIndex(m => m.id === selectedMaterial.value?.id)
    if (materialIndex !== -1) {
      materials.value[materialIndex].matQuantity = newQuantity
    }

    // Get display names
    const selectedEmployee = employees.value.find(e => e.id === formData.value.employee)
    const selectedShift = shifts.value.find(s => s.id === formData.value.shift)
    const selectedProduct = products.value.find(p => p.id === formData.value.product)

    // Add to recent withdrawals
    const withdrawal: Withdrawal = {
      id: Date.now(),
      timestamp: new Date(),
      matName: formData.value.matName,
      quantity: formData.value.quantity,
      machine: formData.value.machine,
      cause: formData.value.cause,
      employee: selectedEmployee?.empId || formData.value.employee,
      employeeName: selectedEmployee?.empName || 'Unknown',
      shift: selectedShift?.shift || 'N/A',
      shiftName: selectedShift?.shiftName || '',
      product: selectedProduct?.product || 'N/A',
      productName: selectedProduct?.product || ''
    }

    recentWithdrawals.value.unshift(withdrawal)
    if (recentWithdrawals.value.length > 10) {
      recentWithdrawals.value = recentWithdrawals.value.slice(0, 10)
    }

    // Save to localStorage
    if (typeof localStorage !== 'undefined') {
      localStorage.setItem('recentWithdrawals', JSON.stringify(recentWithdrawals.value))
    }

    showSuccess(`Successfully withdrew ${formData.value.quantity} ${selectedMaterial.value.matUnit} of ${formData.value.matName}`)

    // Reset quantity
    formData.value.quantity = 0

  } catch (error: any) {
    console.error('Error processing withdrawal:', error)
    showError(error.message || 'Failed to process withdrawal')
  } finally {
    isSubmitting.value = false
  }
}

const initializeData = async (): Promise<void> => {
  try {
    const { error } = await useFetch(`${API_BASE_URL}/initialize-data`, {
      method: 'POST',
      server: false
    })

    if (!error.value) {
      showSuccess('System data initialized successfully')
      await loadMaterials()
      await loadMasterData()
    }
  } catch (error) {
    console.error('Error initializing data:', error)
    showError('Failed to initialize system data')
  }
}

const resetForm = (): void => {
  formData.value = {
    shift: shifts.value[0]?.id || '',
    product: products.value[0]?.id || '',
    matName: '',
    machine: 'LP603',
    quantity: 0,
    cause: 'Add In Loop',
    employee: '',
    materialId: ''
  }
  selectedMaterial.value = null
  clearMessages()
}

const loadRecentWithdrawals = (): void => {
  try {
    if (typeof localStorage !== 'undefined') {
      const saved = localStorage.getItem('recentWithdrawals')
      if (saved) {
        const parsed = JSON.parse(saved)
        recentWithdrawals.value = parsed.map((w: any) => ({
          ...w,
          timestamp: new Date(w.timestamp)
        }))
      }
    }
  } catch (error) {
    console.error('Error loading recent withdrawals:', error)
    recentWithdrawals.value = []
  }
}

const clearMessages = (): void => {
  validationErrors.value = []
  successMessage.value = ''
}

const showSuccess = (message: string): void => {
  successMessage.value = message
  setTimeout(() => {
    successMessage.value = ''
  }, 5000)
}

const showError = (message: string): void => {
  validationErrors.value = [message]
}

const formatDateTime = (date: Date | string | null | undefined): string => {
  if (!date) return 'N/A'
  try {
    return new Date(date).toLocaleString('th-TH')
  } catch {
    return 'N/A'
  }
}

const formatDate = (dateString: string | null | undefined): string => {
  if (!dateString) return 'N/A'
  try {
    return new Date(dateString + 'T00:00:00').toLocaleDateString('th-TH')
  } catch {
    return 'N/A'
  }
}

// Lifecycle
onMounted(async (): Promise<void> => {
  try {
    await loadMasterData()
    await loadMaterials()

    if (materials.value.length === 0) {
      const shouldInitialize = confirm('No materials found in inventory. Would you like to initialize sample data?')
      if (shouldInitialize) {
        await initializeData()
      }
    }

    loadRecentWithdrawals()
  } catch (error) {
    console.error('Error during component initialization:', error)
    showError('Failed to load initial data')
  }
})
</script>

<template>
  <div class="material-withdrawal-page">
    <!-- Header -->
    <div class="header-bar d-flex justify-content-between align-items-center">
      <h4 class="mb-0 text-white fw-bold">Material Withdrawal System</h4>
      <div class="nav-buttons">
        <NuxtLink to="/material-receive" class="btn btn-outline-light btn-sm me-2">
          รับเข้า
        </NuxtLink>
        <button type="button" class="btn-close btn-close-white" aria-label="Close"></button>
      </div>
    </div>

    <!-- Main Form -->
    <div class="form-container">
      <div class="page-title">
        <h5 class="text-danger fw-bold mb-3">เบิกออก - Material Withdrawal</h5>
      </div>

      <form @submit.prevent="handleWithdrawal" class="material-form">
        <!-- Row 1: Shift, Product -->
        <div class="form-row">
          <div class="form-group">
            <label class="form-label">Shift <span class="text-danger">*</span></label>
            <select v-model="formData.shift" class="form-control" required>
              <option value="">Select Shift</option>
              <option v-for="shift in shifts" :key="shift.id" :value="shift.id">
                {{ shift.shift }} - {{ shift.shiftName }}
              </option>
            </select>
          </div>

          <div class="form-group">
            <label class="form-label">Product <span class="text-danger">*</span></label>
            <select v-model="formData.product" class="form-control" required>
              <option value="">Select Product</option>
              <option v-for="product in products" :key="product.id" :value="product.id">
                {{ product.product }}
              </option>
            </select>
          </div>
        </div>

        <!-- Row 2: Mat Name, Machine -->
        <div class="form-row">
          <div class="form-group">
            <label class="form-label">Mat Name <span class="text-danger">*</span></label>
            <select v-model="formData.matName" @change="onMaterialChange" class="form-control" required :disabled="loadingMaterials">
              <option value="">{{ loadingMaterials ? 'Loading...' : 'Select Material' }}</option>
              <option v-for="material in materials" :key="material.id" :value="material.matName">
                {{ material.matName }} ({{ material.matQuantity || 0 }} {{ material.matUnit || 'kg' }})
              </option>
            </select>
          </div>

          <div class="form-group">
            <label class="form-label">Machine <span class="text-danger">*</span></label>
            <select v-model="formData.machine" class="form-control" required>
              <option value="">Select Machine</option>
              <option v-for="machine in DEFAULT_MACHINES" :key="machine" :value="machine">
                {{ machine }}
              </option>
            </select>
          </div>
        </div>

        <!-- Row 3: Cause, Quantity -->
        <div class="form-row">
          <div class="form-group">
            <label class="form-label">Cause/Reason <span class="text-danger">*</span></label>
            <select v-model="formData.cause" class="form-control" required>
              <option value="">Select Cause</option>
              <option v-for="cause in DEFAULT_CAUSES" :key="cause" :value="cause">
                {{ cause }}
              </option>
            </select>
          </div>

          <div class="form-group">
            <label class="form-label">Withdrawal Quantity <span class="text-danger">*</span></label>
            <div class="input-group">
              <input v-model.number="formData.quantity" type="number" step="0.1" min="0" :max="availableStock" class="form-control" placeholder="0.0" required />
              <span class="input-group-text">{{ selectedMaterial?.matUnit || 'kg' }}</span>
            </div>
          </div>
        </div>

        <!-- Row 4: Employee -->
        <div class="form-row">
          <div class="form-group">
            <label class="form-label">Employee <span class="text-danger">*</span></label>
            <select v-model="formData.employee" class="form-control" required>
              <option value="">Select Employee</option>
              <option v-for="employee in employees" :key="employee.id" :value="employee.id">
                {{ employee.empId }} - {{ employee.empName }}
                <span v-if="employee.department">({{ employee.department }})</span>
              </option>
            </select>
          </div>
          <div class="form-group"></div>
        </div>

        <!-- Stock Info -->
        <div class="stock-info-section">
          <div class="form-row">
            <div class="form-group">
              <label class="form-label">Current Balance Stock</label>
              <div class="stock-display">
                <span v-if="selectedMaterial" class="stock-value">
                  {{ selectedMaterial.matQuantity || 0 }} {{ selectedMaterial.matUnit || 'kg' }}
                </span>
                <span v-else class="stock-placeholder">Select material first</span>
              </div>
              <div v-if="selectedMaterial" class="material-details">
                <small class="text-muted">
                  <div>Lot: {{ selectedMaterial.lotNumber || 'N/A' }}</div>
                  <div>Location: {{ selectedMaterial.location || 'N/A' }}</div>
                </small>
              </div>
            </div>

            <div class="form-group">
              <label class="form-label">Refresh Stock Data</label>
              <button type="button" @click="checkStock" :disabled="!formData.matName || loadingStock" class="btn btn-check">
                <span v-if="loadingStock" class="spinner-border spinner-border-sm me-1"></span>
                {{ loadingStock ? 'Checking...' : 'Check Stock' }}
              </button>
            </div>
          </div>
        </div>

        <!-- Alerts -->
        <div v-if="validationErrors.length > 0" class="alert alert-danger">
          <strong>Please fix the following errors:</strong>
          <ul class="mb-0 mt-2">
            <li v-for="error in validationErrors" :key="error">{{ error }}</li>
          </ul>
        </div>

        <div v-if="successMessage" class="alert alert-success">
          <strong>Success!</strong> {{ successMessage }}
        </div>

        <!-- Actions -->
        <div class="action-buttons">
          <button type="submit" :disabled="isSubmitting" class="btn btn-withdraw-primary">
            <span v-if="isSubmitting" class="spinner-border spinner-border-sm me-2"></span>
            {{ isSubmitting ? 'Processing...' : 'เบิกออก (Withdraw)' }}
          </button>

          <button type="button" @click="resetForm" class="btn btn-secondary">
            Reset Form
          </button>

          <button v-if="materials.length === 0" type="button" @click="initializeData" class="btn btn-info">
            Initialize Sample Data
          </button>
        </div>
      </form>
    </div>

    <!-- Recent Withdrawals -->
    <div class="withdrawals-section">
      <div class="section-header">
        <h6 class="section-title">Recent Withdrawals History</h6>
        <span class="record-count">{{ recentWithdrawals.length }} records</span>
      </div>

      <div class="withdrawals-content">
        <div v-if="recentWithdrawals.length === 0" class="no-data">
          <div class="no-data-icon">📦</div>
          <p class="mb-0">No withdrawal records found</p>
        </div>

        <div v-else class="table-container">
          <table class="withdrawals-table">
            <thead>
              <tr>
                <th>Date/Time</th>
                <th>Material</th>
                <th>Quantity</th>
                <th>Machine</th>
                <th>Cause</th>
                <th>Employee</th>
                <th>Shift</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="withdrawal in recentWithdrawals" :key="withdrawal.id">
                <td class="fw-medium">{{ formatDateTime(withdrawal.timestamp) }}</td>
                <td><span class="material-badge">{{ withdrawal.matName }}</span></td>
                <td><span class="quantity-badge">-{{ withdrawal.quantity }}</span></td>
                <td><span class="machine-badge">{{ withdrawal.machine }}</span></td>
                <td class="cause-text">{{ withdrawal.cause }}</td>
                <td>
                  <div class="employee-info">
                    <span class="employee-avatar">{{ withdrawal.employee.charAt(0) }}</span>
                    {{ withdrawal.employeeName }}
                  </div>
                </td>
                <td><span class="shift-badge shift-{{ withdrawal.shift.toLowerCase() }}">{{ withdrawal.shift }}</span></td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* Main Layout */
.material-receive-page {
  min-height: 100vh;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
}

.header-bar {
  background: rgba(0, 0, 0, 0.2);
  backdrop-filter: blur(10px);
  padding: 1rem 1.5rem;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
}

.nav-buttons .btn {
  border-radius: 20px;
  padding: 0.375rem 1rem;
  font-weight: 500;
}

/* Loading State */
.loading-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-height: 400px;
  color: white;
}

.loading-container p {
  margin-top: 1rem;
  font-size: 1.1rem;
}

/* Form Container */
.form-container {
  max-width: 1000px;
  margin: 0 auto;
  padding: 2rem;
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(20px);
  border-radius: 20px;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.1);
  margin-top: 2rem;
  margin-bottom: 2rem;
}

.page-title {
  text-align: center;
  margin-bottom: 2rem;
}

/* Form Styling */
.material-form {
  margin-bottom: 2rem;
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr 1fr;
  gap: 1.5rem;
  margin-bottom: 1.5rem;
}

.form-group {
  display: flex;
  flex-direction: column;
}

.form-label {
  font-weight: 600;
  color: #2d3748;
  margin-bottom: 0.5rem;
  font-size: 0.9rem;
}

.form-control {
  padding: 0.75rem;
  border: 2px solid #e2e8f0;
  border-radius: 8px;
  font-size: 0.95rem;
  transition: all 0.2s ease;
  background: white;
}

.form-control:focus {
  border-color: #4299e1;
  box-shadow: 0 0 0 3px rgba(66, 153, 225, 0.1);
  outline: none;
}

.input-group-text {
  background: #f7fafc;
  border: 2px solid #e2e8f0;
  color: #4a5568;
  font-weight: 500;
}

/* Stock Information Section */
.stock-info-section {
  background: linear-gradient(135deg, #f8fafc 0%, #e2e8f0 100%);
  border-radius: 12px;
  padding: 1.5rem;
  margin: 2rem 0;
  border: 1px solid #cbd5e0;
}

.stock-card {
  text-align: center;
  background: white;
  border-radius: 8px;
  padding: 1rem;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
}

.stock-label {
  display: block;
  font-size: 0.8rem;
  font-weight: 600;
  color: #718096;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  margin-bottom: 0.5rem;
}

.stock-value {
  font-size: 1.5rem;
  font-weight: 700;
  padding: 0.5rem;
  border-radius: 6px;
}

.stock-value.current {
  color: #2b6cb0;
  background: #ebf8ff;
}

.stock-value.receiving {
  color: #38a169;
  background: #f0fff4;
}

.stock-value.new {
  color: #d69e2e;
  background: #fffbeb;
}

.stock-actions {
  margin-top: 1rem;
  text-align: center;
}

.btn-check {
  background: linear-gradient(135deg, #4299e1 0%, #3182ce 100%);
  color: white;
  border: none;
  padding: 0.5rem 1rem;
  border-radius: 6px;
  font-weight: 500;
  transition: all 0.2s ease;
}

.btn-check:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 4px 8px rgba(66, 153, 225, 0.3);
}

.btn-check:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

/* Alert Messages */
.alert {
  border-radius: 8px;
  border: none;
  padding: 1rem;
  margin: 1rem 0;
}

.alert-danger {
  background: linear-gradient(135deg, #fed7d7 0%, #feb2b2 100%);
  color: #c53030;
}

.alert-success {
  background: linear-gradient(135deg, #c6f6d5 0%, #9ae6b4 100%);
  color: #2f855a;
}

.alert-warning {
  background: linear-gradient(135deg, #fef5e7 0%, #febb2b 30%);
  color: #744210;
  border: 1px solid #ed8936;
}

/* Action Buttons */
.action-buttons {
  display: flex;
  gap: 1rem;
  justify-content: center;
  margin-top: 2rem;
}

.btn-receive-primary {
  background: linear-gradient(135deg, #48bb78 0%, #38a169 100%);
  color: white;
  border: none;
  padding: 0.75rem 2rem;
  border-radius: 25px;
  font-weight: 600;
  font-size: 1rem;
  transition: all 0.3s ease;
  box-shadow: 0 4px 15px rgba(72, 187, 120, 0.3);
}

.btn-receive-primary:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 8px 25px rgba(72, 187, 120, 0.4);
}

.btn-receive-primary:disabled {
  opacity: 0.6;
  cursor: not-allowed;
  transform: none;
}

.btn-secondary {
  background: #e2e8f0;
  color: #4a5568;
  border: none;
  padding: 0.75rem 2rem;
  border-radius: 25px;
  font-weight: 500;
  transition: all 0.2s ease;
}

.btn-secondary:hover {
  background: #cbd5e0;
  transform: translateY(-1px);
}

.btn-outline-warning {
  border: 2px solid #ed8936;
  color: #ed8936;
  background: transparent;
  padding: 0.5rem 1rem;
  border-radius: 6px;
  font-weight: 500;
  transition: all 0.2s ease;
}

.btn-outline-warning:hover {
  background: #ed8936;
  color: white;
}

/* Recent Receipts Section */
.receipts-section {
  max-width: 1000px;
  margin: 2rem auto;
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(20px);
  border-radius: 20px;
  overflow: hidden;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.1);
}

.section-header {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  padding: 1rem 1.5rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.section-title {
  margin: 0;
  font-weight: 600;
}

.record-count {
  background: rgba(255, 255, 255, 0.2);
  padding: 0.25rem 0.75rem;
  border-radius: 15px;
  font-size: 0.85rem;
}

.receipts-content {
  padding: 1.5rem;
}

.no-data {
  text-align: center;
  padding: 3rem 2rem;
  color: #718096;
}

.no-data-icon {
  font-size: 3rem;
  margin-bottom: 1rem;
}

/* Table Styling */
.table-container {
  overflow-x: auto;
  border-radius: 8px;
  border: 1px solid #e2e8f0;
}

.receipts-table {
  width: 100%;
  border-collapse: collapse;
  background: white;
}

.receipts-table th {
  background: #f7fafc;
  color: #2d3748;
  font-weight: 600;
  padding: 1rem 0.75rem;
  text-align: left;
  border-bottom: 2px solid #e2e8f0;
  font-size: 0.85rem;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.receipts-table td {
  padding: 0.75rem;
  border-bottom: 1px solid #e2e8f0;
  vertical-align: middle;
}

.receipts-table tbody tr:hover {
  background: #f7fafc;
}

/* Badge Styling */
.material-badge {
  background: #bee3f8;
  color: #2b6cb0;
  padding: 0.25rem 0.5rem;
  border-radius: 4px;
  font-size: 0.8rem;
  font-weight: 500;
}

.quantity-badge {
  padding: 0.25rem 0.5rem;
  border-radius: 4px;
  font-weight: 600;
  font-size: 0.8rem;
}

.quantity-badge.receive {
  background: #c6f6d5;
  color: #2f855a;
}

.supplier-badge {
  background: #fbb6ce;
  color: #b83280;
  padding: 0.25rem 0.5rem;
  border-radius: 4px;
  font-size: 0.8rem;
  font-weight: 500;
}

.location-badge {
  background: #faf089;
  color: #744210;
  padding: 0.25rem 0.5rem;
  border-radius: 4px;
  font-size: 0.8rem;
  font-weight: 500;
}

.shift-badge {
  padding: 0.25rem 0.5rem;
  border-radius: 4px;
  font-size: 0.8rem;
  font-weight: 600;
  text-align: center;
  min-width: 30px;
  display: inline-block;
}

.shift-badge.shift-a {
  background: #fed7d7;
  color: #c53030;
}

.shift-badge.shift-b {
  background: #bee3f8;
  color: #2b6cb0;
}

.shift-badge.shift-c {
  background: #c6f6d5;
  color: #2f855a;
}

.employee-info {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.employee-avatar {
  width: 24px;
  height: 24px;
  border-radius: 50%;
  background: #4299e1;
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.75rem;
  font-weight: 600;
}

.lot-text {
  font-family: 'Courier New', monospace;
  font-size: 0.8rem;
  color: #4a5568;
}

/* Responsive Design */
@media (max-width: 768px) {
  .form-row {
    grid-template-columns: 1fr;
    gap: 1rem;
  }
  
  .form-container {
    margin: 1rem;
    padding: 1rem;
  }
  
  .receipts-section {
    margin: 1rem;
  }
  
  .receipts-table {
    font-size: 0.8rem;
  }
  
  .receipts-table th,
  .receipts-table td {
    padding: 0.5rem 0.25rem;
  }
  
  .action-buttons {
    flex-direction: column;
  }
  
  .stock-info-section .row {
    gap: 1rem;
  }
}

@media (max-width: 576px) {
  .section-header {
    flex-direction: column;
    gap: 0.5rem;
    text-align: center;
  }
  
  .form-row {
    gap: 0.75rem;
  }
  
  .stock-info-section {
    padding: 1rem;
  }
  
  .stock-card {
    padding: 0.75rem;
  }
  
  .stock-value {
    font-size: 1.2rem;
  }
}
</style>