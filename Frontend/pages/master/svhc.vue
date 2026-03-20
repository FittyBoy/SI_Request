<script setup lang="ts">
import {
  FlaskConical,
  Plus,
  Search,
  Database,
  FileText,
  Filter,
  Eye,
  Pencil,
  Trash2,
  X,
  Save,
  Info,
  Beaker,
  Hash,
  Barcode,
  Calendar,
  AlertTriangle,
  BookOpen,
  CheckCircle,
  AlertCircle
} from 'lucide-vue-next'

interface SvhcSubstance {
  Id: string;
  SubstanceName: string;
  CasNo: string;
  EcNo?: string | null;
  ReasonForInclusion?: string | null;
  Uses?: string | null;
  SvhcCandidate?: string | null;
}

interface ApiResponse {
  success: boolean;
  message: string;
  data: SvhcSubstance[];
  totalRecords: number;
  timestamp: string;
}

interface FormData {
  SubstanceName: string;
  CasNo: string;
  EcNo: string;
  ReasonForInclusion: string;
  Uses: string;
  SvhcCandidate: string;
}

// State
const allSubstances = ref<SvhcSubstance[]>([]); // เก็บข้อมูลทั้งหมด
const loading = ref(false);
const showDialog = ref(false);
const showDetailsDialog = ref(false);
const selectedSubstance = ref<SvhcSubstance | null>(null);
const isEditMode = ref(false);
const selectedId = ref<string>('');
const searchQuery = ref('');
const currentPage = ref(1);
const pageSize = ref(50); // ทำเป็น ref เพื่อให้เปลี่ยนได้
const errorMessage = ref('');
const successMessage = ref('');

const formData = ref<FormData>({
  SubstanceName: '',
  CasNo: '',
  EcNo: '',
  ReasonForInclusion: '',
  Uses: '',
  SvhcCandidate: ''
});

// Computed - Filter และ Pagination ฝั่ง Client
const filteredSubstances = computed(() => {
  if (!searchQuery.value) return allSubstances.value;

  const query = searchQuery.value.toLowerCase();
  return allSubstances.value.filter(sub =>
    sub.SubstanceName?.toLowerCase().includes(query) ||
    sub.CasNo?.toLowerCase().includes(query) ||
    sub.EcNo?.toLowerCase().includes(query) ||
    sub.ReasonForInclusion?.toLowerCase().includes(query)
  );
});

const paginatedSubstances = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value;
  const end = start + pageSize.value;
  return filteredSubstances.value.slice(start, end);
});

const totalRecords = computed(() => filteredSubstances.value.length);

const totalPages = computed(() => {
  return Math.ceil(filteredSubstances.value.length / pageSize.value);
});

// Base URL configuration
const config = useRuntimeConfig();
const baseURL = config.public.apiBase;

// Format date for display
const formatDate = (dateString: string | null | undefined) => {
  if (!dateString) return '—';
  try {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' });
  } catch {
    return dateString;
  }
};

// Methods
const loadData = async () => {
  loading.value = true;
  errorMessage.value = '';

  try {
    // เรียกใช้ endpoint ใหม่ที่ดึงข้อมูลทั้งหมด
    const { data, error } = await useFetch<ApiResponse>(`${baseURL}/api/SI25011/svhc/all`);

    if (error.value) {
      errorMessage.value = 'Failed to load SVHC substances';
      console.error('Error loading SVHC substances:', error.value);
      return;
    }

    if (data.value?.success && data.value.data) {
      allSubstances.value = data.value.data;
      // Reset to first page when data is loaded
      currentPage.value = 1;
    } else {
      errorMessage.value = data.value?.message || 'Failed to load SVHC substances';
    }
  } catch (error) {
    errorMessage.value = 'An unexpected error occurred';
    console.error('Error loading SVHC substances:', error);
  } finally {
    loading.value = false;
  }
};

const openAddDialog = () => {
  isEditMode.value = false;
  selectedId.value = '';
  resetForm();
  errorMessage.value = '';
  successMessage.value = '';
  showDialog.value = true;
};

const openEditDialog = (substance: SvhcSubstance) => {
  isEditMode.value = true;
  selectedId.value = substance.Id;
  formData.value = {
    SubstanceName: substance.SubstanceName || '',
    CasNo: substance.CasNo || '',
    EcNo: substance.EcNo || '',
    ReasonForInclusion: substance.ReasonForInclusion || '',
    Uses: substance.Uses || '',
    SvhcCandidate: substance.SvhcCandidate || ''
  };
  errorMessage.value = '';
  successMessage.value = '';
  showDialog.value = true;
};

const openDetailsDialog = (substance: SvhcSubstance) => {
  selectedSubstance.value = substance;
  showDetailsDialog.value = true;
};

const saveData = async () => {
  // Validate all required fields
  if (!formData.value.SubstanceName || 
      !formData.value.CasNo || 
      !formData.value.EcNo || 
      !formData.value.ReasonForInclusion || 
      !formData.value.Uses || 
      !formData.value.SvhcCandidate) {
    errorMessage.value = 'Please fill in all required fields';
    return;
  }

  loading.value = true;
  errorMessage.value = '';
  successMessage.value = '';

  try {
    const url = isEditMode.value
      ? `${baseURL}/api/SI25011/svhc/${selectedId.value}`
      : `${baseURL}/api/SI25011/svhc`;

    const method = isEditMode.value ? 'PUT' : 'POST';

    const payload = {
      SubstanceName: formData.value.SubstanceName,
      CasNo: formData.value.CasNo,
      EcNo: formData.value.EcNo,
      ReasonForInclusion: formData.value.ReasonForInclusion,
      Uses: formData.value.Uses,
      SvhcCandidate: formData.value.SvhcCandidate
    };

    const { data, error } = await useFetch(url, {
      method,
      body: payload
    });

    if (error.value) {
      errorMessage.value = `Failed to ${isEditMode.value ? 'update' : 'create'} SVHC substance`;
      console.error('Error saving SVHC substance:', error.value);
      return;
    }

    if (data.value?.success) {
      successMessage.value = data.value.message || `SVHC substance ${isEditMode.value ? 'updated' : 'created'} successfully`;
      showDialog.value = false;
      await loadData(); // โหลดข้อมูลใหม่

      setTimeout(() => {
        successMessage.value = '';
      }, 3000);
    } else {
      errorMessage.value = data.value?.message || 'Failed to save SVHC substance';
    }
  } catch (error) {
    errorMessage.value = 'An unexpected error occurred';
    console.error('Error saving SVHC substance:', error);
  } finally {
    loading.value = false;
  }
};

const deleteData = async (id: string, name: string) => {
  if (!confirm(`Are you sure you want to delete "${name}"?`)) {
    return;
  }

  loading.value = true;
  errorMessage.value = '';
  successMessage.value = '';

  try {
    const { data, error } = await useFetch(`${baseURL}/api/SI25011/svhc/${id}`, {
      method: 'DELETE'
    });

    if (error.value) {
      errorMessage.value = 'Failed to delete SVHC substance';
      console.error('Error deleting SVHC substance:', error.value);
      return;
    }

    if (data.value?.success) {
      successMessage.value = data.value.message || 'SVHC substance deleted successfully';
      await loadData(); // โหลดข้อมูลใหม่

      setTimeout(() => {
        successMessage.value = '';
      }, 3000);
    } else {
      errorMessage.value = data.value?.message || 'Failed to delete SVHC substance';
    }
  } catch (error) {
    errorMessage.value = 'An unexpected error occurred';
    console.error('Error deleting SVHC substance:', error);
  } finally {
    loading.value = false;
  }
};

const resetForm = () => {
  formData.value = {
    SubstanceName: '',
    CasNo: '',
    EcNo: '',
    ReasonForInclusion: '',
    Uses: '',
    SvhcCandidate: ''
  };
};

// Watch for search query changes - reset to page 1
watch(searchQuery, () => {
  currentPage.value = 1;
});

// Watch for page size changes - reset to page 1
watch(pageSize, () => {
  currentPage.value = 1;
});

onMounted(() => {
  loadData();
});
</script>

<template>
  <div class="substance-master-container">
    <v-container fluid class="pa-6">
      <!-- Header Section -->
      <v-row class="mb-6">
        <v-col cols="12">
          <div class="header-section">
            <div class="d-flex align-center">
              <div class="icon-wrapper mr-4">
                <AlertTriangle :size="40" class="text-warning" />
              </div>
              <div>
                <h1 class="text-h4 font-weight-bold mb-1">REACH SVHC Master</h1>
                <p class="text-subtitle-1 text-medium-emphasis">Manage Substances of Very High Concern</p>
              </div>
            </div>
          </div>
        </v-col>
      </v-row>

      <!-- Success/Error Messages -->
      <v-row v-if="successMessage || errorMessage" class="mb-4">
        <v-col cols="12">
          <v-slide-y-transition>
            <v-alert v-if="successMessage" type="success" variant="tonal" closable prominent class="mb-4"
              @click:close="successMessage = ''">
              <template #prepend>
                <CheckCircle :size="24" />
              </template>
              {{ successMessage }}
            </v-alert>
          </v-slide-y-transition>

          <v-slide-y-transition>
            <v-alert v-if="errorMessage" type="error" variant="tonal" closable prominent
              @click:close="errorMessage = ''">
              <template #prepend>
                <AlertCircle :size="24" />
              </template>
              {{ errorMessage }}
            </v-alert>
          </v-slide-y-transition>
        </v-col>
      </v-row>

      <!-- Main Card -->
      <v-card elevation="2" class="main-card">
        <!-- Search and Actions Bar -->
        <v-card-text class="pa-6">
          <v-row class="align-center">
            <v-col cols="12" md="8" lg="9">
              <v-text-field v-model="searchQuery" variant="outlined" density="comfortable"
                label="Search SVHC substances" placeholder="Search by substance name, CAS number, EC number, or reason..."
                clearable hide-details bg-color="surface">
                <template #prepend-inner>
                  <Search :size="20" class="text-medium-emphasis" />
                </template>
              </v-text-field>
            </v-col>
            <v-col cols="12" md="4" lg="3" class="text-right">
              <v-btn color="warning" size="large" @click="openAddDialog" :disabled="loading" elevation="0"
                class="text-none font-weight-medium">
                <Plus :size="20" class="mr-2" />
                Add New SVHC
              </v-btn>
            </v-col>
          </v-row>

          <!-- Stats Section -->
          <v-row class="mt-4">
            <v-col cols="12" sm="4">
              <v-card variant="tonal" color="warning" class="stats-card">
                <v-card-text class="text-center">
                  <Database :size="32" class="mb-2" />
                  <div class="text-h5 font-weight-bold">{{ allSubstances.length }}</div>
                  <div class="text-caption text-medium-emphasis">Total SVHC</div>
                </v-card-text>
              </v-card>
            </v-col>
            <v-col cols="12" sm="4">
              <v-card variant="tonal" color="success" class="stats-card">
                <v-card-text class="text-center">
                  <FileText :size="32" class="mb-2" />
                  <div class="text-h5 font-weight-bold">{{ currentPage }} / {{ totalPages }}</div>
                  <div class="text-caption text-medium-emphasis">Current Page</div>
                </v-card-text>
              </v-card>
            </v-col>
            <v-col cols="12" sm="4">
              <v-card variant="tonal" color="info" class="stats-card">
                <v-card-text class="text-center">
                  <Filter :size="32" class="mb-2" />
                  <div class="text-h5 font-weight-bold">{{ totalRecords }}</div>
                  <div class="text-caption text-medium-emphasis">Filtered Results</div>
                </v-card-text>
              </v-card>
            </v-col>
          </v-row>
        </v-card-text>

        <v-divider />

        <!-- Data Table -->
        <v-card-text class="pa-0">
          <v-data-table :headers="[
            { title: 'Substance Name', key: 'SubstanceName', sortable: true },
            { title: 'CAS Number', key: 'CasNo', sortable: true },
            { title: 'EC Number', key: 'EcNo', sortable: true },
            { title: 'SVHC Candidate Date', key: 'SvhcCandidate', sortable: true },
            { title: 'Actions', key: 'actions', sortable: false, align: 'center', width: 180 }
          ]" :items="paginatedSubstances" :loading="loading" :items-per-page="-1" hide-default-footer
            class="elevation-0 custom-table" hover>
            <template #item.SubstanceName="{ item }">
              <div class="py-3">
                <div class="font-weight-bold text-body-1">{{ item.SubstanceName }}</div>
                <div v-if="item.ReasonForInclusion" class="text-caption text-medium-emphasis mt-1">
                  {{ item.ReasonForInclusion.substring(0, 50) }}{{ item.ReasonForInclusion.length > 50 ? '...' : '' }}
                </div>
              </div>
            </template>

            <template #item.CasNo="{ item }">
              <span class="text-mono">{{ item.CasNo }}</span>
            </template>

            <template #item.EcNo="{ item }">
              <v-chip v-if="item.EcNo" size="small" color="primary" variant="tonal">
                {{ item.EcNo }}
              </v-chip>
              <span v-else class="text-medium-emphasis">—</span>
            </template>

            <template #item.SvhcCandidate="{ item }">
              <div v-if="item.SvhcCandidate" class="d-flex align-center">
                <Calendar :size="16" class="mr-2 text-medium-emphasis" />
                <span class="text-body-2">{{ formatDate(item.SvhcCandidate) }}</span>
              </div>
              <span v-else class="text-medium-emphasis">—</span>
            </template>

            <template #item.actions="{ item }">
              <div class="action-buttons">
                <v-btn icon size="small" variant="text" color="info" @click="openDetailsDialog(item)"
                  title="View Details" :disabled="loading">
                  <Eye :size="18" />
                </v-btn>
                <v-btn icon size="small" variant="text" color="warning" @click="openEditDialog(item)" title="Edit"
                  :disabled="loading">
                  <Pencil :size="18" />
                </v-btn>
                <v-btn icon size="small" variant="text" color="error" @click="deleteData(item.Id, item.SubstanceName)"
                  title="Delete" :disabled="loading">
                  <Trash2 :size="18" />
                </v-btn>
              </div>
            </template>

            <template #no-data>
              <div class="text-center pa-8">
                <AlertTriangle :size="64" class="text-grey-lighten-2 mb-4" />
                <div class="text-h6 mt-4 text-medium-emphasis">
                  {{ loading ? 'Loading...' : searchQuery ? 'No matching SVHC substances found' : 'No SVHC substances found' }}
                </div>
                <div v-if="!loading && searchQuery" class="text-body-2 text-medium-emphasis mt-2">
                  Try adjusting your search terms
                </div>
              </div>
            </template>

            <template #loading>
              <div class="text-center pa-8">
                <v-progress-circular indeterminate color="warning" size="48" />
                <div class="text-body-1 mt-4 text-medium-emphasis">Loading SVHC substances...</div>
              </div>
            </template>

            <template #bottom>
              <div class="pagination-container">
                <v-divider />
                <div class="d-flex align-center justify-space-between pa-4 flex-wrap">
                  <div class="d-flex align-center gap-4">
                    <div class="text-body-2 text-medium-emphasis">
                      Showing {{ ((currentPage - 1) * pageSize) + 1 }} to {{ Math.min(currentPage * pageSize, totalRecords) }} of {{ totalRecords }} entries
                      <span v-if="searchQuery">(filtered from {{ allSubstances.length }} total)</span>
                    </div>
                    <v-select
                      v-model="pageSize"
                      :items="[10, 25, 50, 100, 200]"
                      density="compact"
                      variant="outlined"
                      hide-details
                      style="max-width: 100px;"
                      label="Per page"
                    />
                  </div>
                  <v-pagination 
                    v-model="currentPage" 
                    :length="totalPages" 
                    :total-visible="7" 
                    size="small"
                    density="comfortable" 
                  />
                </div>
              </div>
            </template>
          </v-data-table>
        </v-card-text>
      </v-card>
    </v-container>

    <!-- Add/Edit Dialog -->
    <v-dialog v-model="showDialog" max-width="900px" persistent scrollable>
      <v-card class="dialog-card">
        <v-card-title class="d-flex align-center pa-6 bg-warning">
          <div class="icon-wrapper-white mr-3">
            <component :is="isEditMode ? Pencil : Plus" :size="24" />
          </div>
          <span class="text-h5">
            {{ isEditMode ? 'Edit SVHC Substance' : 'Add New SVHC Substance' }}
          </span>
          <v-spacer />
          <v-btn icon variant="text" @click="showDialog = false" :disabled="loading">
            <X :size="20" />
          </v-btn>
        </v-card-title>

        <v-divider />

        <v-card-text class="pa-6">
          <v-alert v-if="errorMessage" type="error" variant="tonal" closable prominent class="mb-6"
            @click:close="errorMessage = ''">
            {{ errorMessage }}
          </v-alert>

          <v-form>
            <v-row>
              <v-col cols="12">
                <div class="text-h6 mb-4 font-weight-medium">
                  <Info :size="20" class="mr-2 inline-icon" />
                  Basic Information
                </div>
              </v-col>

              <v-col cols="12">
                <v-text-field 
                  v-model="formData.SubstanceName" 
                  label="Substance Name" 
                  variant="outlined"
                  density="comfortable" 
                  required 
                  :error="!formData.SubstanceName" 
                  hint="Maximum 500 characters"
                  persistent-hint
                  :rules="[v => !!v || 'Substance Name is required']"
                >
                  <template #prepend-inner>
                    <Beaker :size="20" class="text-medium-emphasis" />
                  </template>
                  <template #append-inner>
                    <v-chip size="x-small" color="error" variant="flat">Required</v-chip>
                  </template>
                </v-text-field>
              </v-col>

              <v-col cols="12" md="6">
                <v-text-field 
                  v-model="formData.CasNo" 
                  label="CAS Number" 
                  variant="outlined" 
                  density="comfortable"
                  required 
                  :error="!formData.CasNo" 
                  hint="Maximum 50 characters" 
                  persistent-hint
                  :rules="[v => !!v || 'CAS Number is required']"
                >
                  <template #prepend-inner>
                    <Barcode :size="20" class="text-medium-emphasis" />
                  </template>
                  <template #append-inner>
                    <v-chip size="x-small" color="error" variant="flat">Required</v-chip>
                  </template>
                </v-text-field>
              </v-col>

              <v-col cols="12" md="6">
                <v-text-field 
                  v-model="formData.EcNo" 
                  label="EC Number" 
                  variant="outlined" 
                  density="comfortable"
                  required 
                  :error="!formData.EcNo" 
                  hint="Maximum 50 characters" 
                  persistent-hint
                  :rules="[v => !!v || 'EC Number is required']"
                >
                  <template #prepend-inner>
                    <Hash :size="20" class="text-medium-emphasis" />
                  </template>
                  <template #append-inner>
                    <v-chip size="x-small" color="error" variant="flat">Required</v-chip>
                  </template>
                </v-text-field>
              </v-col>

              <v-col cols="12">
                <v-text-field 
                  v-model="formData.SvhcCandidate" 
                  label="SVHC Candidate Date" 
                  variant="outlined"
                  density="comfortable" 
                  type="date" 
                  required 
                  :error="!formData.SvhcCandidate" 
                  hint="Date when added to candidate list" 
                  persistent-hint
                  :rules="[v => !!v || 'SVHC Candidate Date is required']"
                >
                  <template #prepend-inner>
                    <Calendar :size="20" class="text-medium-emphasis" />
                  </template>
                  <template #append-inner>
                    <v-chip size="x-small" color="error" variant="flat">Required</v-chip>
                  </template>
                </v-text-field>
              </v-col>

              <v-col cols="12">
                <v-divider class="my-4" />
              </v-col>

              <v-col cols="12">
                <div class="text-h6 mb-4 font-weight-medium">
                  <FileText :size="20" class="mr-2 inline-icon" />
                  Additional Details
                </div>
              </v-col>

              <v-col cols="12">
                <v-textarea 
                  v-model="formData.ReasonForInclusion" 
                  label="Reason for Inclusion" 
                  variant="outlined"
                  rows="3" 
                  required 
                  :error="!formData.ReasonForInclusion" 
                  hint="Why this substance is included in SVHC list (max 500 characters)"
                  persistent-hint
                  :rules="[v => !!v || 'Reason for Inclusion is required']"
                >
                  <template #prepend-inner>
                    <AlertTriangle :size="20" class="text-medium-emphasis" />
                  </template>
                  <template #append-inner>
                    <v-chip size="x-small" color="error" variant="flat">Required</v-chip>
                  </template>
                </v-textarea>
              </v-col>

              <v-col cols="12">
                <v-textarea 
                  v-model="formData.Uses" 
                  label="Uses" 
                  variant="outlined" 
                  rows="3"
                  required 
                  :error="!formData.Uses"
                  hint="Known uses of this substance (max 1000 characters)" 
                  persistent-hint
                  :rules="[v => !!v || 'Uses is required']"
                >
                  <template #prepend-inner>
                    <BookOpen :size="20" class="text-medium-emphasis" />
                  </template>
                  <template #append-inner>
                    <v-chip size="x-small" color="error" variant="flat">Required</v-chip>
                  </template>
                </v-textarea>
              </v-col>
            </v-row>
          </v-form>
        </v-card-text>

        <v-divider />

        <v-card-actions class="pa-6">
          <v-spacer />
          <v-btn color="grey" variant="text" size="large" @click="showDialog = false" :disabled="loading"
            class="text-none px-6">
            Cancel
          </v-btn>
          <v-btn color="warning" variant="elevated" size="large" @click="saveData" :loading="loading"
            :disabled="!formData.SubstanceName || !formData.CasNo || !formData.EcNo || !formData.ReasonForInclusion || !formData.Uses || !formData.SvhcCandidate" 
            class="text-none px-8">
            <Save :size="20" class="mr-2" />
            {{ isEditMode ? 'Update' : 'Save' }}
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Details Dialog -->
    <v-dialog v-model="showDetailsDialog" max-width="800px">
      <v-card v-if="selectedSubstance">
        <v-card-title class="d-flex align-center pa-6 bg-warning">
          <div class="icon-wrapper-white mr-3">
            <Info :size="24" />
          </div>
          <span class="text-h5">SVHC Substance Details</span>
          <v-spacer />
          <v-btn icon variant="text" @click="showDetailsDialog = false">
            <X :size="20" />
          </v-btn>
        </v-card-title>

        <v-divider />

        <v-card-text class="pa-6">
          <v-list lines="two">
            <v-list-item>
              <template #prepend>
                <div class="icon-wrapper-warning">
                  <Beaker :size="20" />
                </div>
              </template>
              <v-list-item-title class="font-weight-bold">Substance Name</v-list-item-title>
              <v-list-item-subtitle>{{ selectedSubstance.SubstanceName }}</v-list-item-subtitle>
            </v-list-item>

            <v-divider class="my-2" />

            <v-list-item>
              <template #prepend>
                <div class="icon-wrapper-warning">
                  <Barcode :size="20" />
                </div>
              </template>
              <v-list-item-title class="font-weight-bold">CAS Number</v-list-item-title>
              <v-list-item-subtitle>{{ selectedSubstance.CasNo }}</v-list-item-subtitle>
            </v-list-item>

            <v-divider class="my-2" />

            <v-list-item v-if="selectedSubstance.EcNo">
              <template #prepend>
                <div class="icon-wrapper-warning">
                  <Hash :size="20" />
                </div>
              </template>
              <v-list-item-title class="font-weight-bold">EC Number</v-list-item-title>
              <v-list-item-subtitle>{{ selectedSubstance.EcNo }}</v-list-item-subtitle>
            </v-list-item>

            <v-divider v-if="selectedSubstance.EcNo" class="my-2" />

            <v-list-item v-if="selectedSubstance.SvhcCandidate">
              <template #prepend>
                <div class="icon-wrapper-warning">
                  <Calendar :size="20" />
                </div>
              </template>
              <v-list-item-title class="font-weight-bold">SVHC Candidate Date</v-list-item-title>
              <v-list-item-subtitle>{{ formatDate(selectedSubstance.SvhcCandidate) }}</v-list-item-subtitle>
            </v-list-item>

            <v-divider v-if="selectedSubstance.SvhcCandidate" class="my-2" />

            <v-list-item v-if="selectedSubstance.ReasonForInclusion">
              <template #prepend>
                <div class="icon-wrapper-warning">
                  <AlertTriangle :size="20" />
                </div>
              </template>
              <v-list-item-title class="font-weight-bold">Reason for Inclusion</v-list-item-title>
              <v-list-item-subtitle class="text-wrap">{{ selectedSubstance.ReasonForInclusion }}</v-list-item-subtitle>
            </v-list-item>

            <v-divider v-if="selectedSubstance.ReasonForInclusion" class="my-2" />

            <v-list-item v-if="selectedSubstance.Uses">
              <template #prepend>
                <div class="icon-wrapper-warning">
                  <BookOpen :size="20" />
                </div>
              </template>
              <v-list-item-title class="font-weight-bold">Uses</v-list-item-title>
              <v-list-item-subtitle class="text-wrap">{{ selectedSubstance.Uses }}</v-list-item-subtitle>
            </v-list-item>
          </v-list>
        </v-card-text>

        <v-divider />

        <v-card-actions class="pa-6">
          <v-spacer />
          <v-btn color="warning" variant="text" size="large" @click="showDetailsDialog = false" class="text-none px-6">
            Close
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </div>
</template>

<style scoped>
.substance-master-container {
  background: var(--bg);
  min-height: 100vh;
  padding: var(--s-6) 0;
}
.header-section {
  background: var(--surface); padding: var(--s-6) var(--s-8);
  border-radius: var(--r-xl); box-shadow: var(--shadow-md); border: 1px solid var(--border);
  position: relative; overflow: hidden;
}
.header-section::before {
  content: ''; position: absolute; top: 0; left: 0; right: 0; height: 4px;
  background: linear-gradient(90deg, var(--brand) 0%, var(--dept-qa) 100%);
}
.main-card {
  border-radius: var(--r-xl) !important; overflow: hidden;
  border: 1px solid var(--border) !important; box-shadow: var(--shadow-sm) !important;
}
.stats-card {
  transition: transform var(--t-mid) var(--ease), box-shadow var(--t-mid) var(--ease);
  border-radius: var(--r-lg) !important; border: 1px solid var(--border) !important;
}
.stats-card:hover { transform: translateY(-4px); box-shadow: var(--shadow-lg) !important; }
.custom-table :deep(.v-data-table__tr:hover) { background-color: var(--brand-xlight) !important; }
.action-buttons { display: flex; gap: var(--s-2); justify-content: center; }
.dialog-card { border-radius: var(--r-xl) !important; }
.text-mono { font-family: var(--font-mono); font-size: var(--fs-sm); }
.icon-wrapper         { display: inline-flex; align-items: center; justify-content: center; }
.icon-wrapper-white   { display: inline-flex; align-items: center; justify-content: center; color: var(--text-1); }
.icon-wrapper-primary {
  display: inline-flex; align-items: center; justify-content: center;
  color: rgb(var(--v-theme-primary)); background-color: rgba(var(--v-theme-primary), 0.1);
  padding: var(--s-2); border-radius: var(--r-md);
}
.icon-wrapper-warning {
  display: inline-flex; align-items: center; justify-content: center;
  color: var(--warning); background: var(--warning-bg);
  padding: var(--s-2); border-radius: var(--r-md);
}
:deep(.v-data-table) { border-radius: 0 !important; }
:deep(.v-data-table-header th) { font-weight: 700 !important; text-transform: uppercase !important; font-size: var(--fs-xs) !important; letter-spacing: 0.06em !important; background: var(--surface-3) !important; color: var(--text-2) !important; }
:deep(.v-data-table__td), :deep(.v-data-table__th) { padding: 14px var(--s-4) !important; font-size: var(--fs-sm) !important; }
:deep(.v-pagination__item) { box-shadow: none !important; }
:deep(.v-pagination) { justify-content: flex-end; }
:deep(.v-overlay__content)::-webkit-scrollbar { width: 5px; }
:deep(.v-overlay__content)::-webkit-scrollbar-thumb { background: var(--border); border-radius: var(--r-full); }
:deep(.v-field:hover) { box-shadow: var(--shadow-sm); }
.v-alert { animation: slideIn 0.3s var(--ease); }
@keyframes slideIn { from { opacity: 0; transform: translateY(-8px); } to { opacity: 1; transform: translateY(0); } }
@media (max-width: 960px) {
  .substance-master-container { padding: var(--s-3) 0; }
  .header-section { padding: var(--s-4); }
  :deep(.v-pagination) { justify-content: center !important; }
}
</style>
