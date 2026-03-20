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
.material-receive-page { min-height: 100vh; background: var(--paper-2); }

.header-bar {
  background: linear-gradient(135deg, var(--dp-master) 0%, #14532d 100%);
  padding: var(--sp-4) var(--sp-8); position: sticky; top:0; z-index:100;
  box-shadow: 0 4px 20px rgba(22,163,74,.25);
}
.nav-buttons .btn { border-radius: var(--rr-max); padding: var(--sp-1) var(--sp-4); font-weight: 600; }

.loading-container { display:flex; flex-direction:column; align-items:center; justify-content:center; min-height:400px; color: var(--ink-3); }
.loading-container p { margin-top: var(--sp-4); }

.form-container {
  max-width:1000px; margin: var(--sp-8) auto; padding: var(--sp-8);
  background: var(--paper); border-radius: var(--rr-xl); box-shadow: var(--el-3);
  position: relative; overflow: hidden;
}
.form-container::before {
  content:''; position:absolute; top:0; left:0; right:0; height:4px;
  background: linear-gradient(90deg, var(--dp-master) 0%, #4ade80 100%);
}

.page-title { text-align:center; margin-bottom: var(--sp-8); }
.form-row { display:grid; grid-template-columns:1fr 1fr 1fr; gap: var(--sp-5); margin-bottom: var(--sp-5); }
.form-group { display:flex; flex-direction:column; }
.form-label { font-size: var(--fz-2xs); font-weight:700; color: var(--ink-3); text-transform:uppercase; letter-spacing:.07em; margin-bottom:6px; }
.form-control {
  padding:10px var(--sp-3); border:1.5px solid var(--line); border-radius: var(--rr-md);
  background: var(--paper); color: var(--ink); font-size: var(--fz-sm); font-family: var(--f-sans); outline:none;
  transition: border-color var(--dur-fast) var(--ease-out), box-shadow var(--dur-fast) var(--ease-out);
}
.form-control:focus { border-color: var(--dp-master); box-shadow:0 0 0 3px rgba(22,163,74,.15); }
.input-group { display:flex; }
.input-group .form-control { border-right:none; border-radius: var(--rr-md) 0 0 var(--rr-md); }
.input-group-text { display:flex; align-items:center; padding:10px var(--sp-3); background: var(--paper-3); border:1.5px solid var(--line); border-left:none; border-radius:0 var(--rr-md) var(--rr-md) 0; font-size: var(--fz-sm); color: var(--ink-3); font-weight:500; }

.stock-info-section { background: var(--paper-2); padding: var(--sp-6); border-radius: var(--rr-lg); border:1.5px dashed var(--line-2); margin: var(--sp-6) 0; }
.stock-card { text-align:center; background: var(--paper); border-radius: var(--rr-md); padding: var(--sp-4); box-shadow: var(--el-1); }
.stock-label { display:block; font-size: var(--fz-2xs); font-weight:700; color: var(--ink-3); text-transform:uppercase; letter-spacing:.07em; margin-bottom: var(--sp-2); }
.stock-value { font-size: var(--fz-2xl); font-weight:700; padding: var(--sp-2); border-radius: var(--rr-sm); }
.stock-value.current   { color: var(--info);    background: var(--info-bg); }
.stock-value.receiving { color: var(--ok);      background: var(--ok-bg); }
.stock-value.new       { color: var(--warn);    background: var(--warn-bg); }
.stock-placeholder { color: var(--ink-4); font-size: var(--fz-sm); font-style:italic; }
.stock-actions { margin-top: var(--sp-4); text-align:center; }

.btn-check { background: var(--brand); color:#fff; border:none; padding:10px var(--sp-4); border-radius: var(--rr-md); font-weight:600; font-family: var(--f-sans); cursor:pointer; transition: all var(--dur-fast) var(--ease-out); }
.btn-check:hover:not(:disabled) { background: var(--brand-2); transform:translateY(-1px); }
.btn-check:disabled { opacity:.4; cursor:not-allowed; }

.alert { border-radius: var(--rr-md); padding: var(--sp-4) var(--sp-5); margin: var(--sp-4) 0; }
.alert-danger  { background: var(--fail-bg); border-left:4px solid var(--fail); color: var(--fail); }
.alert-success { background: var(--ok-bg);   border-left:4px solid var(--ok);   color: var(--ok); }
.alert-warning { background: var(--warn-bg); border-left:4px solid var(--warn); color: var(--warn); }

.action-buttons { display:flex; gap: var(--sp-5); justify-content:center; margin-top: var(--sp-8); }
.btn { padding: var(--sp-3) var(--sp-8); border:none; border-radius: var(--rr-max); font-size: var(--fz-md); font-weight:700; cursor:pointer; font-family: var(--f-sans); transition: all var(--dur-mid) var(--ease-out); display:flex; align-items:center; justify-content:center; gap: var(--sp-2); }
.btn:disabled { opacity:.4; cursor:not-allowed; }
.btn-receive-primary { background: linear-gradient(135deg, var(--dp-master) 0%, #14532d 100%); color:#fff; box-shadow:0 4px 16px rgba(22,163,74,.3); }
.btn-receive-primary:hover:not(:disabled) { transform:translateY(-2px); box-shadow:0 8px 24px rgba(22,163,74,.4); }
.btn-secondary { background: var(--paper-3); color: var(--ink); border:1.5px solid var(--line); box-shadow:none; }
.btn-secondary:hover { background: var(--line); }
.btn-outline-warning { border:1.5px solid var(--warn); color: var(--warn); background:transparent; border-radius: var(--rr-md); padding: var(--sp-2) var(--sp-4); cursor:pointer; font-weight:600; transition: all var(--dur-fast) var(--ease-out); }
.btn-outline-warning:hover { background: var(--warn); color:#fff; }

.receipts-section { max-width:1000px; margin: var(--sp-8) auto; background: var(--paper); border-radius: var(--rr-xl); overflow:hidden; box-shadow: var(--el-3); }
.section-header { background: linear-gradient(135deg, var(--dp-master) 0%, #14532d 100%); color:#fff; padding: var(--sp-4) var(--sp-6); display:flex; justify-content:space-between; align-items:center; }
.section-title { margin:0; font-weight:700; font-size: var(--fz-md); }
.record-count { background:rgba(255,255,255,.2); padding:2px var(--sp-3); border-radius: var(--rr-max); font-size: var(--fz-2xs); font-weight:700; }
.receipts-content { padding: var(--sp-6); }
.no-data { text-align:center; padding: var(--sp-10) var(--sp-6); color: var(--ink-3); }
.no-data-icon { font-size:3rem; margin-bottom: var(--sp-4); }

.table-container { overflow-x:auto; border-radius: var(--rr-md); box-shadow: var(--el-1); }
.receipts-table { width:100%; border-collapse:collapse; background: var(--paper); }
.receipts-table th { background: var(--paper-3); color: var(--ink-3); font-weight:700; padding: var(--sp-3) var(--sp-4); text-align:left; border-bottom:2px solid var(--line); font-size: var(--fz-2xs); text-transform:uppercase; letter-spacing:.07em; }
.receipts-table td { padding:14px var(--sp-3); border-bottom:1px solid var(--line); color: var(--ink); vertical-align:middle; }
.receipts-table tbody tr:hover td { background: var(--brand-bg); }

.material-badge { background: var(--info-bg);  color: var(--info);  padding:3px var(--sp-2); border-radius: var(--rr-max); font-size: var(--fz-2xs); font-weight:700; border:1px solid var(--info-mid); }
.quantity-badge.receive { background: var(--ok-bg); color: var(--ok); padding:3px var(--sp-2); border-radius: var(--rr-max); font-size: var(--fz-2xs); font-weight:700; border:1px solid var(--ok-mid); }
.supplier-badge { background:#fce7f3; color:#9d174d; padding:3px var(--sp-2); border-radius: var(--rr-max); font-size: var(--fz-2xs); font-weight:700; border:1px solid #fbcfe8; }
.location-badge { background: var(--warn-bg); color: var(--warn); padding:3px var(--sp-2); border-radius: var(--rr-max); font-size: var(--fz-2xs); font-weight:700; border:1px solid var(--warn-mid); }
.shift-badge { padding:3px var(--sp-2); border-radius: var(--rr-max); font-size: var(--fz-2xs); font-weight:700; text-align:center; min-width:26px; display:inline-block; }
.shift-badge.shift-a { background: var(--fail-bg); color: var(--fail); }
.shift-badge.shift-b { background: var(--info-bg); color: var(--info); }
.shift-badge.shift-c { background: var(--ok-bg);   color: var(--ok); }
.employee-info { display:flex; align-items:center; gap: var(--sp-2); }
.employee-avatar { width:28px; height:28px; border-radius:50%; background: linear-gradient(135deg, var(--dp-master) 0%, #14532d 100%); color:#fff; display:flex; align-items:center; justify-content:center; font-size: var(--fz-2xs); font-weight:700; }
.lot-text { font-family: var(--f-mono); font-size: var(--fz-xs); color: var(--ink-3); }

@media (max-width:768px) {
  .form-row { grid-template-columns:1fr; gap: var(--sp-4); }
  .form-container, .receipts-section { margin: var(--sp-4); padding: var(--sp-5); }
  .action-buttons { flex-direction:column; align-items:center; }
}
</style>
