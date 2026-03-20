<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'

// SEO และ Meta
useHead({
  title: 'Material Withdrawal - LP Control System',
  meta: [
    { name: 'description', content: 'Material withdrawal system for LP manufacturing' },
    { name: 'viewport', content: 'width=device-width, initial-scale=1' }
  ]
})

// Type definitions - ปรับให้ตรงกับ Backend Model
interface MateralInventory {
  Id: string
  MatName: string
  MatQuantity: number | null
  matUnit?: string
  matTypeId?: string
  case?: string
  expDate: string | null  // DateOnly จะกลายเป็น string ใน JSON
  insertDate: string | null  // DateOnly จะกลายเป็น string ใน JSON
  empId?: string
  shift?: string
  product?: string
  supplier?: string
  lotNumber?: string
  Location?: string
}

interface WithdrawalRequest {
  shift: string
  product: string
  MatName: string
  machine: string
  quantity: number
  cause: string
  employee: string
  materialId: string
}

interface Withdrawal {
  id: number
  timestamp: Date
  MatName: string
  quantity: number
  machine: string
  cause: string
  employee: string
  shift: string
  product: string
}

// Reactive state
const formData = ref<WithdrawalRequest>({
  shift: 'B',
  product: 'Mobile 0.1',
  MatName: 'FO1500',
  machine: 'LP603',
  quantity: 0.5,
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
const Locations = ref<any[]>([])
const employees = ref<any[]>([])

// API Configuration from runtime config
const { public: { apiBase } } = useRuntimeConfig()

// Computed properties
const availableStock = computed((): number => {
  return selectedMaterial.value?.MatQuantity || 0
})

// Static constants (fallback if API fails)
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

// Methods
const loadMasterData = async (): Promise<void> => {
  try {
    // Load products
    const { data: productsData, error: productsError } = await useFetch<any[]>(`${apiBase}/api/SI25006/products`, {
      server: false,
      default: () => []
    })

    if (!productsError.value && productsData.value) {
      products.value = productsData.value
    }

    // Load shifts
    const { data: shiftsData, error: shiftsError } = await useFetch<any[]>(`${apiBase}/api/SI25006/shifts`, {
      server: false,
      default: () => []
    })

    if (!shiftsError.value && shiftsData.value) {
      shifts.value = shiftsData.value
    }

    // Load suppliers
    const { data: suppliersData, error: suppliersError } = await useFetch<any[]>(`${apiBase}/api/SI25006/suppliers`, {
      server: false,
      default: () => []
    })

    if (!suppliersError.value && suppliersData.value) {
      suppliers.value = suppliersData.value
    }

    // Load Locations
    const { data: LocationsData, error: LocationsError } = await useFetch<any[]>(`${apiBase}/api/SI25006/Locations`, {
      server: false,
      default: () => []
    })

    if (!LocationsError.value && LocationsData.value) {
      Locations.value = LocationsData.value
    }

    // Load employees
    const { data: employeesData, error: employeesError } = await useFetch<any[]>(`${apiBase}/api/SI25006/employees`, {
      server: false,
      default: () => []
    })

    if (!employeesError.value && employeesData.value) {
      employees.value = employeesData.value
    }

  } catch (error) {
    console.error('Error loading master data:', error)
    // Fallback to default values if API fails
    products.value = [
      { product: 'Mobile 0.1', count: 0 },
      { product: 'Mobile 0.2', count: 0 },
      { product: 'Mobile 0.3', count: 0 }
    ]
    shifts.value = [
      { shift: 'A', count: 0 },
      { shift: 'B', count: 0 },
      { shift: 'C', count: 0 }
    ]
  }
}

const loadMaterials = async (): Promise<void> => {
  loadingMaterials.value = true
  try {
    const { data: materialsData, error } = await useFetch<MateralInventory[]>(`${apiBase}/api/SI25006`, {
      server: false,
      default: () => []
    })

    if (!error.value && materialsData.value) {
      materials.value = materialsData.value.map(material => ({
        ...material,
        matUnit: material.matUnit || 'kg', // Default unit if not specified
        MatQuantity: material.MatQuantity || 0
      }))

      // Set default material if available
      if (materials.value.length > 0 && !selectedMaterial.value) {
        const defaultMat = materials.value.find(m => m.MatName === 'FO1500') || materials.value[0]
        selectedMaterial.value = defaultMat
        formData.value.MatName = defaultMat.MatName
        formData.value.materialId = defaultMat.Id
      }
    } else {
      throw new Error(error.value?.message || 'Failed to load materials')
    }
  } catch (error) {
    console.error('Error loading materials:', error)
    showError('Failed to load materials from server')
  } finally {
    loadingMaterials.value = false
  }
}

const onMaterialChange = (): void => {
  const selected = materials.value.find(m => m.MatName === formData.value.MatName)
  selectedMaterial.value = selected || null
  formData.value.materialId = selected?.Id || ''
  clearMessages()
}

const checkStock = async (): Promise<void> => {
  if (!selectedMaterial.value) return

  loadingStock.value = true
  try {
    // Refresh individual material data
    const { data: materialData, error } = await useFetch<MateralInventory>(`${apiBase}/api/SI25006/${selectedMaterial.value.Id}`, {
      server: false
    })

    if (!error.value && materialData.value) {
      // Update local material data
      const materialIndex = materials.value.findIndex(m => m.Id === selectedMaterial.value?.Id)
      if (materialIndex !== -1) {
        materials.value[materialIndex] = {
          ...materialData.value,
          matUnit: materialData.value.matUnit || 'kg',
          MatQuantity: materialData.value.MatQuantity || 0
        }
        selectedMaterial.value = materials.value[materialIndex]
      }

      showSuccess('Stock data refreshed successfully')
    } else {
      throw new Error(error.value?.message || 'Failed to fetch material data')
    }
  } catch (error) {
    console.error('Error checking stock:', error)
    // Fallback to reloading all materials if individual fetch fails
    try {
      await loadMaterials()
      showSuccess('Stock data refreshed from main inventory')
    } catch (fallbackError) {
      showError('Failed to refresh stock data')
    }
  } finally {
    loadingStock.value = false
  }
}

const validateForm = (): boolean => {
  validationErrors.value = []

  if (!formData.value.shift) validationErrors.value.push('Please select a shift')
  if (!formData.value.product) validationErrors.value.push('Please select a product')
  if (!formData.value.MatName) validationErrors.value.push('Please select a material')
  if (!formData.value.machine) validationErrors.value.push('Please select a machine')
  if (!formData.value.cause) validationErrors.value.push('Please select a cause')
  if (!formData.value.employee) validationErrors.value.push('Please enter employee ID')
  if (!formData.value.quantity || formData.value.quantity <= 0) {
    validationErrors.value.push('Please enter a valid quantity')
  }

  if (selectedMaterial.value && formData.value.quantity > (selectedMaterial.value.MatQuantity || 0)) {
    validationErrors.value.push(
      `Insufficient stock. Available: ${selectedMaterial.value.MatQuantity} ${selectedMaterial.value.matUnit}`
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
    // Calculate new quantity
    const currentQuantity = selectedMaterial.value.MatQuantity || 0
    const newQuantity = currentQuantity - formData.value.quantity

    if (newQuantity < 0) {
      throw new Error('Insufficient stock for withdrawal')
    }

    // Update material quantity via API - ใช้ useFetch แทน $fetch
    const { error } = await useFetch(`${apiBase}//api/SI25006/${selectedMaterial.value.Id}/quantity`, {
      method: 'PATCH',
      body: newQuantity,
      server: false
    })

    if (error.value) {
      throw new Error(error.value.message || 'Failed to update material quantity')
    }

    // TODO: เมื่อมี Withdrawal API จึงจะใช้
    // สำหรับตอนนี้ comment ไว้ก่อนเนื่องจาก API ยังไม่มี endpoint นี้
    /*
    const withdrawalRecord = {
      materialId: selectedMaterial.value.id,
      MatName: formData.value.MatName,
      quantity: formData.value.quantity,
      machine: formData.value.machine,
      cause: formData.value.cause,
      empId: formData.value.employee,
      shift: formData.value.shift,
      product: formData.value.product,
      withdrawalDate: new Date().toISOString()
    }

    const { error: withdrawalError } = await useFetch(`${apiBase}/api/SI25006/withdrawal`, {
      method: 'POST',
      body: withdrawalRecord,
      server: false
    })

    if (withdrawalError.value) {
      throw new Error(withdrawalError.value.message || 'Failed to record withdrawal')
    }
    */

    // Update local state
    selectedMaterial.value.MatQuantity = newQuantity
    const materialIndex = materials.value.findIndex(m => m.Id === selectedMaterial.value?.Id)
    if (materialIndex !== -1) {
      materials.value[materialIndex].MatQuantity = newQuantity
    }

    // Add to recent withdrawals (local storage for demo)
    const withdrawal: Withdrawal = {
      id: Date.now(),
      timestamp: new Date(),
      MatName: formData.value.MatName,
      quantity: formData.value.quantity,
      machine: formData.value.machine,
      cause: formData.value.cause,
      employee: formData.value.employee,
      shift: formData.value.shift,
      product: formData.value.product
    }

    recentWithdrawals.value.unshift(withdrawal)
    if (recentWithdrawals.value.length > 10) {
      recentWithdrawals.value = recentWithdrawals.value.slice(0, 10)
    }

    // Save to localStorage for persistence (เนื่องจากยังไม่มี withdrawal API)
    if (typeof localStorage !== 'undefined') {
      localStorage.setItem('recentWithdrawals', JSON.stringify(recentWithdrawals.value))
    }

    showSuccess(`Successfully withdrew ${formData.value.quantity} ${selectedMaterial.value.matUnit} of ${formData.value.MatName}`)

    // Reset quantity
    formData.value.quantity = 0

  } catch (error: any) {
    console.error('Error processing withdrawal:', error)
    if (error && typeof error === 'object' && 'status' in error) {
      const status = error.status
      if (status === 404) {
        showError('Material not found')
      } else if (status === 400) {
        showError('Invalid request data')
      } else if (status >= 500) {
        showError('Server error. Please try again later.')
      } else {
        showError('Failed to process withdrawal. Please try again.')
      }
    } else {
      showError('Network error. Please check your connection.')
    }
  } finally {
    isSubmitting.value = false
  }
}

const initializeData = async (): Promise<void> => {
  try {
    const { error } = await useFetch(`${apiBase}/api/SI25006/initialize-data`, {
      method: 'POST',
      server: false
    })

    if (!error.value) {
      showSuccess('System data initialized successfully')
      // Reload materials after initialization
      await loadMaterials()
      await loadMasterData()
    } else {
      throw new Error(error.value.message || 'Failed to initialize data')
    }
  } catch (error) {
    console.error('Error initializing data:', error)
    showError('Failed to initialize system data')
  }
}

const resetForm = (): void => {
  formData.value = {
    shift: '',
    product: '',
    MatName: '',
    machine: '',
    quantity: 0,
    cause: '',
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
    // DateOnly จาก C# จะมาเป็น "YYYY-MM-DD" format
    return new Date(dateString + 'T00:00:00').toLocaleDateString('th-TH')
  } catch {
    return 'N/A'
  }
}

// Lifecycle
onMounted(async (): Promise<void> => {
  try {
    // Load master data first
    await loadMasterData()

    // Load materials from API
    await loadMaterials()

    // If no materials found, offer to initialize
    if (materials.value.length === 0) {
      const shouldInitialize = confirm('No materials found in inventory. Would you like to initialize sample data?')
      if (shouldInitialize) {
        await initializeData()
      }
    }

    // Load recent withdrawals from localStorage
    loadRecentWithdrawals()
  } catch (error) {
    console.error('Error during component initialization:', error)
    showError('Failed to load initial data. Some features may not work properly.')
  }
})
</script>

<template>
  <div class="material-withdrawal-page">
    <!-- Header with Navigation -->
    <div class="header-bar d-flex justify-content-between align-items-center">
      <h4 class="mb-0 text-white fw-bold">Material Withdrawal System</h4>
      <div class="nav-buttons">
        <NuxtLink to="/material-receive" class="btn btn-outline-light btn-sm me-2">
          รับเข้า
        </NuxtLink>
        <button type="button" class="btn-close btn-close-white" aria-label="Close"></button>
      </div>
    </div>

    <!-- Main Form Container -->
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
              <option v-for="shift in shifts" :key="shift.shift" :value="shift.shift">
                {{ shift.shift }} ({{ shift.count }} employees)
              </option>
              <!-- Fallback options -->
              <option v-if="shifts.length === 0" value="A">A</option>
              <option v-if="shifts.length === 0" value="B">B</option>
              <option v-if="shifts.length === 0" value="C">C</option>
            </select>
          </div>

          <div class="form-group">
            <label class="form-label">Product <span class="text-danger">*</span></label>
            <select v-model="formData.product" class="form-control" required>
              <option value="">Select Product</option>
              <option v-for="product in products" :key="product.product" :value="product.product">
                {{ product.product }}
                <span v-if="product.count !== undefined">({{ product.count }} materials)</span>
              </option>
              <!-- Fallback options -->
              <option v-if="products.length === 0" value="Mobile 0.1">Mobile 0.1</option>
              <option v-if="products.length === 0" value="Mobile 0.2">Mobile 0.2</option>
              <option v-if="products.length === 0" value="Mobile 0.3">Mobile 0.3</option>
            </select>
          </div>
        </div>

        <!-- Row 2: Mat Name, Machine -->
        <div class="form-row">
          <div class="form-group">
            <label class="form-label">Mat Name <span class="text-danger">*</span></label>
            <select v-model="formData.MatName" @change="onMaterialChange" class="form-control" required
              :disabled="loadingMaterials">
              <option value="">{{ loadingMaterials ? 'Loading materials...' : 'Select Material' }}</option>
              <option v-for="material in materials" :key="material.Id" :value="material.MatName">
                {{ material.MatName }} ({{ material.MatQuantity || 0 }} {{ material.matUnit || 'kg' }})
                <span v-if="material.Location"> - {{ material.Location }}</span>
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

        <!-- Row 3: Cause/Reason, Withdrawal Quantity -->
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
              <input v-model.number="formData.quantity" type="number" step="0.1" min="0" :max="availableStock"
                class="form-control" placeholder="0.0" required />
              <span class="input-group-text">{{ selectedMaterial?.matUnit || 'kg' }}</span>
            </div>
          </div>
        </div>

        <div class="form-row">
          <div class="form-group">
            <label class="form-label">Employee ID <span class="text-danger">*</span></label>
            <select v-model="formData.employee" class="form-control" required>
              <option value="">Select Employee</option>
              <option v-for="employee in employees" :key="employee.empId" :value="employee.empId">
                {{ employee.empId }} - {{ employee.empName || employee.employeename }}
                <span v-if="employee.department">({{ employee.department }})</span>
              </option>
            </select>
            <!-- Fallback input if no employees loaded -->
            <input v-if="employees.length === 0" v-model="formData.employee" type="text" class="form-control mt-2"
              placeholder="Enter Employee ID manually" />
          </div>

          <!-- Empty space to maintain layout -->
          <div class="form-group"></div>
        </div>

        <!-- Stock Information -->
        <div class="stock-info-section">
          <div class="form-row">
            <div class="form-group">
              <label class="form-label">Current Balance Stock</label>
              <div class="stock-display">
                <span v-if="selectedMaterial" class="stock-value">
                  {{ selectedMaterial.MatQuantity || 0 }} {{ selectedMaterial.matUnit || 'kg' }}
                </span>
                <span v-else-if="loadingMaterials" class="stock-placeholder">Loading...</span>
                <span v-else class="stock-placeholder">Select material first</span>
              </div>
              <!-- แสดงข้อมูลเพิ่มเติม -->
              <div v-if="selectedMaterial" class="material-details">
                <small class="text-muted">
                  <div v-if="selectedMaterial.lotNumber">Lot: {{ selectedMaterial.lotNumber }}</div>
                  <div v-if="selectedMaterial.Location">Location: {{ selectedMaterial.Location }}</div>
                  <div v-if="selectedMaterial.expDate">Exp: {{ formatDate(selectedMaterial.expDate) }}</div>
                  <div v-if="selectedMaterial.supplier">Supplier: {{ selectedMaterial.supplier }}</div>
                </small>
              </div>
            </div>

            <div class="form-group">
              <label class="form-label">Refresh Stock Data</label>
              <button type="button" @click="checkStock" :disabled="!formData.MatName || loadingStock"
                class="btn btn-check">
                <span v-if="loadingStock" class="spinner-border spinner-border-sm me-1"></span>
                {{ loadingStock ? 'Checking...' : 'Check Stock' }}
              </button>
            </div>
          </div>
        </div>

        <!-- Alert Messages -->
        <div v-if="validationErrors.length > 0" class="alert alert-danger">
          <strong>Please fix the following errors:</strong>
          <ul class="mb-0 mt-2">
            <li v-for="error in validationErrors" :key="error">{{ error }}</li>
          </ul>
        </div>

        <div v-if="successMessage" class="alert alert-success">
          <strong>Success!</strong> {{ successMessage }}
        </div>

        <!-- Action Buttons -->
        <div class="action-buttons">
          <button type="submit" :disabled="isSubmitting" class="btn btn-withdraw-primary">
            <span v-if="isSubmitting" class="spinner-border spinner-border-sm me-2"></span>
            {{ isSubmitting ? 'Processing Withdrawal...' : 'เบิกออก (Withdraw)' }}
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

    <!-- Recent Withdrawals Section -->
    <div class="withdrawals-section">
      <div class="section-header">
        <h6 class="section-title">Recent Withdrawals History</h6>
        <span class="record-count">{{ recentWithdrawals.length }} records</span>
      </div>

      <div class="withdrawals-content">
        <div v-if="recentWithdrawals.length === 0" class="no-data">
          <div class="no-data-icon">📦</div>
          <p class="mb-0">No withdrawal records found</p>
          <small class="text-muted">Start withdrawing materials to see history</small>
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
                <td>
                  <span class="material-badge">{{ withdrawal.MatName }}</span>
                </td>
                <td>
                  <span class="quantity-badge">-{{ withdrawal.quantity }}</span>
                </td>
                <td>
                  <span class="machine-badge">{{ withdrawal.machine }}</span>
                </td>
                <td class="cause-text">{{ withdrawal.cause }}</td>
                <td>
                  <div class="employee-info">
                    <span class="employee-avatar">{{ withdrawal.employee.charAt(0) }}</span>
                    {{ withdrawal.employee }}
                  </div>
                </td>
                <td>
                  <span class="shift-badge shift-{{ withdrawal.shift.toLowerCase() }}">{{ withdrawal.shift }}</span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.material-withdrawal-page { min-height:100vh; background: var(--paper-2); }

.header-bar {
  background: linear-gradient(135deg, var(--dp-lapping) 0%, #991b1b 100%);
  padding: var(--sp-4) var(--sp-8); position:sticky; top:0; z-index:100;
  box-shadow:0 4px 20px rgba(239,68,68,.25);
}
.header-bar h4 { color:#fff; font-size: var(--fz-lg); font-weight:700; letter-spacing:-.02em; }
.nav-buttons { display:flex; align-items:center; gap: var(--sp-2); }
.btn-close { width:32px; height:32px; background:rgba(255,255,255,.15); border:1.5px solid rgba(255,255,255,.3); border-radius: var(--rr-md); color:#fff; font-size:18px; cursor:pointer; display:flex; align-items:center; justify-content:center; transition: background var(--dur-fast) var(--ease-out); }
.btn-close:hover { background:rgba(255,255,255,.25); }
.btn-close::before { content:"×"; font-weight:bold; }

.form-container { padding: var(--sp-8); max-width:1100px; margin:0 auto; }

.page-title { text-align:center; margin-bottom: var(--sp-8); padding: var(--sp-5) var(--sp-6); background: var(--paper); border-radius: var(--rr-lg); border-left:5px solid var(--dp-lapping); box-shadow: var(--el-2); }
.page-title h5 { color: var(--dp-lapping); font-size: var(--fz-xl); font-weight:700; margin:0; letter-spacing:-.02em; }

.material-form { background: var(--paper); padding: var(--sp-8); border-radius: var(--rr-xl); box-shadow: var(--el-3); position:relative; overflow:hidden; }
.material-form::before { content:''; position:absolute; top:0; left:0; right:0; height:4px; background: linear-gradient(90deg, var(--dp-lapping) 0%, #fca5a5 100%); }

.form-row { display:flex; gap: var(--sp-5); margin-bottom: var(--sp-5); align-items:flex-end; }
.form-group { flex:1; display:flex; flex-direction:column; }
.form-label { font-size: var(--fz-2xs); font-weight:700; color: var(--ink-3); text-transform:uppercase; letter-spacing:.07em; margin-bottom:6px; }
.form-control { padding:10px var(--sp-3); border:1.5px solid var(--line); border-radius: var(--rr-md); background: var(--paper); color: var(--ink); font-size: var(--fz-sm); font-family: var(--f-sans); height:42px; outline:none; transition: border-color var(--dur-fast) var(--ease-out), box-shadow var(--dur-fast) var(--ease-out); }
.form-control:focus { border-color: var(--dp-lapping); box-shadow:0 0 0 3px rgba(239,68,68,.15); }
.input-group { display:flex; }
.input-group .form-control { border-right:none; border-radius: var(--rr-md) 0 0 var(--rr-md); }
.input-group-text { display:flex; align-items:center; background: var(--paper-3); border:1.5px solid var(--line); border-left:none; border-radius:0 var(--rr-md) var(--rr-md) 0; padding:0 var(--sp-3); font-size: var(--fz-sm); color: var(--ink-3); font-weight:500; }
.form-text { font-size: var(--fz-xs); color: var(--ink-4); margin-top: var(--sp-1); }

.stock-info-section { background: var(--paper-2); padding: var(--sp-6); border-radius: var(--rr-lg); border:1.5px dashed var(--line-2); margin: var(--sp-6) 0; }
.stock-display { padding: var(--sp-3) var(--sp-4); background: var(--ok-bg); border:1.5px solid var(--ok-mid); border-radius: var(--rr-md); height:42px; display:flex; align-items:center; }
.stock-value { color: var(--ok); font-weight:700; font-size: var(--fz-md); }
.stock-placeholder { color: var(--ink-4); font-size: var(--fz-sm); font-style:italic; }
.material-details { margin-top: var(--sp-2); padding: var(--sp-2) var(--sp-3); background: var(--paper); border-radius: var(--rr-sm); border:1px solid var(--line); font-size: var(--fz-xs); color: var(--ink-2); }

.btn-check { width:100%; padding:10px var(--sp-4); background: var(--brand); color:#fff; border:none; border-radius: var(--rr-md); cursor:pointer; font-size: var(--fz-sm); font-weight:600; height:42px; font-family: var(--f-sans); transition: all var(--dur-fast) var(--ease-out); }
.btn-check:hover:not(:disabled) { background: var(--brand-2); transform:translateY(-1px); }
.btn-check:disabled { opacity:.4; cursor:not-allowed; }

.action-buttons { display:flex; justify-content:center; gap: var(--sp-5); margin: var(--sp-8) 0; }
.btn { padding: var(--sp-3) var(--sp-8); border:none; border-radius: var(--rr-max); font-size: var(--fz-md); font-weight:700; cursor:pointer; min-width:160px; font-family: var(--f-sans); transition: all var(--dur-mid) var(--ease-out); display:flex; align-items:center; justify-content:center; gap: var(--sp-2); }
.btn:disabled { opacity:.4; cursor:not-allowed; }
.btn-withdraw-primary { background: linear-gradient(135deg, var(--dp-lapping) 0%, #991b1b 100%); color:#fff; box-shadow:0 4px 16px rgba(239,68,68,.3); }
.btn-withdraw-primary:hover:not(:disabled) { transform:translateY(-2px); box-shadow:0 8px 24px rgba(239,68,68,.4); }
.btn-secondary { background: var(--paper-3); color: var(--ink); border:1.5px solid var(--line); }
.btn-secondary:hover:not(:disabled) { background: var(--line); }

.alert { margin: var(--sp-5) 0; padding: var(--sp-4) var(--sp-5); border-radius: var(--rr-md); }
.alert-danger  { background: var(--fail-bg); border-left:4px solid var(--fail); color: var(--fail); }
.alert-success { background: var(--ok-bg);   border-left:4px solid var(--ok);   color: var(--ok); }
.alert ul { padding-left: var(--sp-5); margin-top: var(--sp-2); }

.withdrawals-section { margin-top: var(--sp-8); background: var(--paper); border-radius: var(--rr-xl); overflow:hidden; box-shadow: var(--el-3); }
.section-header { display:flex; justify-content:space-between; align-items:center; padding: var(--sp-5) var(--sp-6); background: var(--paper-3); border-bottom:1px solid var(--line); }
.section-title { margin:0; color: var(--ink); font-size: var(--fz-md); font-weight:700; }
.record-count { background: var(--dp-lapping); color:#fff; padding:2px var(--sp-3); border-radius: var(--rr-max); font-size: var(--fz-2xs); font-weight:700; }
.withdrawals-content { padding: var(--sp-6); }
.no-data { text-align:center; padding: var(--sp-10) 0; color: var(--ink-3); }
.no-data-icon { font-size:48px; margin-bottom: var(--sp-4); }

.table-container { overflow-x:auto; border-radius: var(--rr-md); }
.withdrawals-table { width:100%; border-collapse:collapse; background: var(--paper); font-size: var(--fz-sm); }
.withdrawals-table th { background: var(--paper-3); padding: var(--sp-3) var(--sp-4); text-align:left; font-weight:700; font-size: var(--fz-2xs); text-transform:uppercase; letter-spacing:.07em; color: var(--ink-3); border-bottom:2px solid var(--line); }
.withdrawals-table td { padding:14px var(--sp-4); border-bottom:1px solid var(--line); color: var(--ink); }
.withdrawals-table tbody tr:hover td { background: var(--brand-bg); }

.material-badge { background: var(--brand-bg); color: var(--brand); padding:3px var(--sp-2); border-radius: var(--rr-max); font-size: var(--fz-2xs); font-weight:700; border:1px solid var(--brand-mid); }
.quantity-badge  { background: var(--fail-bg);  color: var(--fail);  padding:3px var(--sp-2); border-radius: var(--rr-max); font-size: var(--fz-2xs); font-weight:700; border:1px solid var(--fail-mid); }
.machine-badge   { background: var(--warn-bg);  color: var(--warn);  padding:3px var(--sp-2); border-radius: var(--rr-max); font-size: var(--fz-2xs); font-weight:700; border:1px solid var(--warn-mid); }
.cause-text { font-size: var(--fz-xs); color: var(--ink-3); }
.employee-info { display:flex; align-items:center; gap: var(--sp-2); }
.employee-avatar { width:30px; height:30px; background: linear-gradient(135deg, var(--dp-lapping) 0%, #991b1b 100%); color:#fff; border-radius:50%; display:flex; align-items:center; justify-content:center; font-size: var(--fz-2xs); font-weight:700; flex-shrink:0; }
.shift-badge { padding:3px var(--sp-2); border-radius: var(--rr-max); font-size: var(--fz-2xs); font-weight:700; text-align:center; min-width:26px; display:inline-block; }
.shift-a { background: var(--fail-bg); color: var(--fail); }
.shift-b { background: var(--info-bg); color: var(--info); }
.shift-c { background: var(--ok-bg);   color: var(--ok); }

@media (max-width:768px) {
  .form-row { flex-direction:column; gap: var(--sp-4); }
  .form-container { padding: var(--sp-4); }
  .material-form { padding: var(--sp-5); }
  .action-buttons { flex-direction:column; align-items:center; }
  .btn { width:100%; max-width:320px; }
}
</style>
