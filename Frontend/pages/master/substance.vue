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
  Gauge,
  FileType,
  Lightbulb,
  BookOpen,
  CheckCircle,
  AlertCircle
} from 'lucide-vue-next'

interface Substance {
  Id: string;
  SubstanceChemical: string;
  SubstanceIdentifier: string;
  SubstanceCasNo: string;
  SubstanceThresholdLimit?: string | null;
  SubstanceScope?: string | null;
  SubstanceExamples?: string | null;
  SubstanceReferences?: string | null;
}

interface FormData {
  SubstanceChemical: string;
  SubstanceIdentifier: string;
  SubstanceCasNo: string;
  SubstanceThresholdLimit: string;
  SubstanceScope: string;
  SubstanceExamples: string;
  SubstanceReferences: string;
}

// State
const allSubstances = ref<Substance[]>([]); // เก็บข้อมูลทั้งหมด
const loading = ref(false);
const showDialog = ref(false);
const showDetailsDialog = ref(false);
const selectedSubstance = ref<Substance | null>(null);
const isEditMode = ref(false);
const selectedId = ref<string>('');
const searchQuery = ref('');
const currentPage = ref(1);
const itemsPerPage = ref(50);
const errorMessage = ref('');
const successMessage = ref('');

const formData = ref<FormData>({
  SubstanceChemical: '',
  SubstanceIdentifier: '',
  SubstanceCasNo: '',
  SubstanceThresholdLimit: '',
  SubstanceScope: '',
  SubstanceExamples: '',
  SubstanceReferences: ''
});

// Computed - Filter ข้อมูล
const filteredSubstances = computed(() => {
  if (!searchQuery.value) return allSubstances.value;

  const query = searchQuery.value.toLowerCase();
  return allSubstances.value.filter(sub =>
    sub.SubstanceChemical?.toLowerCase().includes(query) ||
    sub.SubstanceCasNo?.toLowerCase().includes(query) ||
    sub.SubstanceIdentifier?.toLowerCase().includes(query) ||
    sub.SubstanceThresholdLimit?.toLowerCase().includes(query) ||
    sub.SubstanceScope?.toLowerCase().includes(query)
  );
});

// Computed - Pagination
const totalPages = computed(() => {
  return Math.ceil(filteredSubstances.value.length / itemsPerPage.value);
});

const paginatedSubstances = computed(() => {
  const start = (currentPage.value - 1) * itemsPerPage.value;
  const end = start + itemsPerPage.value;
  return filteredSubstances.value.slice(start, end);
});

const totalRecords = computed(() => allSubstances.value.length);

// Base URL configuration
const config = useRuntimeConfig();
const baseURL = config.public.apiBase;

// Methods
const loadData = async () => {
  loading.value = true;
  errorMessage.value = '';

  try {
    // เรียก API แบบไม่มี pagination
    const { data, error } = await useFetch<any>(`${baseURL}/api/SI25011/substances/all`, {
      key: `all-substances-${Date.now()}`,
      server: false,
      lazy: false,
      watch: false
    });

    if (error.value) {
      errorMessage.value = 'Failed to load substances';
      console.error('Error loading substances:', error.value);
      return;
    }

    const isSuccess = data.value?.success || data.value?.Success;
    const responseData = data.value?.data || data.value?.Data;

    if (isSuccess && responseData) {
      allSubstances.value = responseData;
      // Reset ไปหน้าแรกเมื่อโหลดข้อมูลใหม่
      currentPage.value = 1;
    } else {
      errorMessage.value = data.value?.message || data.value?.Message || 'Failed to load substances';
    }
  } catch (error) {
    errorMessage.value = 'An unexpected error occurred';
    console.error('Error loading substances:', error);
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

const openEditDialog = (substance: Substance) => {
  isEditMode.value = true;
  selectedId.value = substance.Id;
  formData.value = {
    SubstanceChemical: substance.SubstanceChemical || '',
    SubstanceIdentifier: substance.SubstanceIdentifier || '',
    SubstanceCasNo: substance.SubstanceCasNo || '',
    SubstanceThresholdLimit: substance.SubstanceThresholdLimit || '',
    SubstanceScope: substance.SubstanceScope || '',
    SubstanceExamples: substance.SubstanceExamples || '',
    SubstanceReferences: substance.SubstanceReferences || ''
  };
  errorMessage.value = '';
  successMessage.value = '';
  showDialog.value = true;
};

const openDetailsDialog = (substance: Substance) => {
  selectedSubstance.value = substance;
  showDetailsDialog.value = true;
};

const saveData = async () => {
  if (!formData.value.SubstanceChemical || !formData.value.SubstanceIdentifier || !formData.value.SubstanceCasNo) {
    errorMessage.value = 'Please fill in all required fields (Chemical Name, Identifier, CAS Number)';
    return;
  }

  loading.value = true;
  errorMessage.value = '';
  successMessage.value = '';

  try {
    const url = isEditMode.value
      ? `${baseURL}/api/SI25011/substances/${selectedId.value}`
      : `${baseURL}/api/SI25011/substances`;

    const method = isEditMode.value ? 'PUT' : 'POST';

    const { data, error } = await useFetch<any>(url, {
      method,
      body: formData.value,
      key: `save-substance-${Date.now()}`,
      server: false,
      watch: false
    });

    if (error.value) {
      errorMessage.value = `Failed to ${isEditMode.value ? 'update' : 'create'} substance`;
      console.error('Error saving substance:', error.value);
      loading.value = false;
      return;
    }

    const isSuccess = data.value?.Success || data.value?.success;
    const message = data.value?.Message || data.value?.message;

    if (isSuccess) {
      showDialog.value = false;
      successMessage.value = message || `Substance ${isEditMode.value ? 'updated' : 'created'} successfully`;

      await nextTick();

      // โหลดข้อมูลใหม่ทั้งหมด
      await loadData();

      setTimeout(() => {
        successMessage.value = '';
      }, 3000);
    } else {
      errorMessage.value = message || 'Failed to save substance';
    }
  } catch (error) {
    errorMessage.value = 'An unexpected error occurred';
    console.error('Error saving substance:', error);
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
    const { data, error } = await useFetch<any>(`${baseURL}/api/SI25011/substances/${id}`, {
      method: 'DELETE',
      key: `delete-substance-${id}-${Date.now()}`,
      server: false,
      watch: false
    });

    if (error.value) {
      errorMessage.value = 'Failed to delete substance';
      console.error('Error deleting substance:', error.value);
      return;
    }

    const isSuccess = data.value?.Success || data.value?.success;
    const message = data.value?.Message || data.value?.message;

    if (isSuccess) {
      successMessage.value = message || 'Substance deleted successfully';
      await loadData();

      setTimeout(() => {
        successMessage.value = '';
      }, 3000);
    } else {
      errorMessage.value = message || 'Failed to delete substance';
    }
  } catch (error) {
    errorMessage.value = 'An unexpected error occurred';
    console.error('Error deleting substance:', error);
  } finally {
    loading.value = false;
  }
};

const resetForm = () => {
  formData.value = {
    SubstanceChemical: '',
    SubstanceIdentifier: '',
    SubstanceCasNo: '',
    SubstanceThresholdLimit: '',
    SubstanceScope: '',
    SubstanceExamples: '',
    SubstanceReferences: ''
  };
};

// Watch searchQuery - Reset หน้าเมื่อ search
watch(searchQuery, () => {
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
                <FlaskConical :size="40" class="text-primary" />
              </div>
              <div>
                <h1 class="text-h4 font-weight-bold mb-1">Regular Substance Master</h1>
                <p class="text-subtitle-1 text-medium-emphasis">Manage chemical substances and their properties</p>
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
              <v-text-field v-model="searchQuery" variant="outlined" density="comfortable" label="Search substances"
                placeholder="Search by chemical name, CAS number, identifier, threshold, or scope..." clearable
                hide-details bg-color="surface">
                <template #prepend-inner>
                  <Search :size="20" class="text-medium-emphasis" />
                </template>
              </v-text-field>
            </v-col>
            <v-col cols="12" md="4" lg="3" class="text-right">
              <v-btn color="primary" size="large" @click="openAddDialog" :disabled="loading" elevation="0"
                class="text-none font-weight-medium">
                <Plus :size="20" class="mr-2" />
                Add New Substance
              </v-btn>
            </v-col>
          </v-row>

          <!-- Stats Section -->
          <v-row class="mt-4">
            <v-col cols="12" sm="4">
              <v-card variant="tonal" color="primary" class="stats-card">
                <v-card-text class="text-center">
                  <Database :size="32" class="mb-2" />
                  <div class="text-h5 font-weight-bold">{{ totalRecords }}</div>
                  <div class="text-caption text-medium-emphasis">Total Substances</div>
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
                  <div class="text-h5 font-weight-bold">{{ filteredSubstances.length }}</div>
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
            { title: 'Chemical Name', key: 'SubstanceChemical', sortable: true },
            { title: 'Identifier', key: 'SubstanceIdentifier', sortable: true },
            { title: 'CAS Number', key: 'SubstanceCasNo', sortable: true },
            { title: 'Threshold Limit', key: 'SubstanceThresholdLimit', sortable: false },
            { title: 'Actions', key: 'actions', sortable: false, align: 'center', width: 180 }
          ]" :items="paginatedSubstances" :loading="loading" :items-per-page="-1" hide-default-footer
            class="elevation-0 custom-table" hover>
            <template #item.SubstanceChemical="{ item }">
              <div class="py-3">
                <div class="font-weight-bold text-body-1">{{ item.SubstanceChemical }}</div>
              </div>
            </template>

            <template #item.SubstanceIdentifier="{ item }">
              <v-chip size="small" color="indigo" variant="flat">
                {{ item.SubstanceIdentifier }}
              </v-chip>
            </template>

            <template #item.SubstanceCasNo="{ item }">
              <span class="text-mono">{{ item.SubstanceCasNo }}</span>
            </template>

            <template #item.SubstanceThresholdLimit="{ item }">
              <span v-if="item.SubstanceThresholdLimit" class="text-body-2">
                {{ item.SubstanceThresholdLimit }}
              </span>
              <span v-else class="text-medium-emphasis">—</span>
            </template>

            <template #item.actions="{ item }">
              <div class="action-buttons">
                <v-btn icon size="small" variant="text" color="info" @click="openDetailsDialog(item)"
                  title="View Details" :disabled="loading">
                  <Eye :size="18" />
                </v-btn>
                <v-btn icon size="small" variant="text" color="primary" @click="openEditDialog(item)" title="Edit"
                  :disabled="loading">
                  <Pencil :size="18" />
                </v-btn>
                <v-btn icon size="small" variant="text" color="error"
                  @click="deleteData(item.Id, item.SubstanceChemical)" title="Delete" :disabled="loading">
                  <Trash2 :size="18" />
                </v-btn>
              </div>
            </template>

            <template #no-data>
              <div class="text-center pa-8">
                <FlaskConical :size="64" class="text-grey-lighten-2 mb-4" />
                <div class="text-h6 mt-4 text-medium-emphasis">
                  {{ loading ? 'Loading...' : searchQuery ? 'No matching substances found' : 'No substances found' }}
                </div>
                <div v-if="!loading && searchQuery" class="text-body-2 text-medium-emphasis mt-2">
                  Try adjusting your search terms
                </div>
              </div>
            </template>

            <template #loading>
              <div class="text-center pa-8">
                <v-progress-circular indeterminate color="primary" size="48" />
                <div class="text-body-1 mt-4 text-medium-emphasis">Loading substances...</div>
              </div>
            </template>

            <template #bottom>
              <div class="pagination-container">
                <v-divider />
                <div class="d-flex align-center justify-space-between pa-4">
                  <div class="text-body-2 text-medium-emphasis">
                    Showing {{ ((currentPage - 1) * itemsPerPage) + 1 }}
                    to {{ Math.min(currentPage * itemsPerPage, filteredSubstances.length) }}
                    of {{ filteredSubstances.length }} results
                    <span v-if="searchQuery"> (filtered from {{ totalRecords }} total)</span>
                  </div>
                  <v-pagination v-model="currentPage" :length="totalPages" :total-visible="7" size="small"
                    density="comfortable" />
                </div>
              </div>
            </template>
          </v-data-table>
        </v-card-text>
      </v-card>
    </v-container>

    <!-- Add/Edit Dialog (same as before) -->
    <v-dialog v-model="showDialog" max-width="900px" persistent scrollable>
      <v-card class="dialog-card">
        <v-card-title class="d-flex align-center pa-6 bg-primary">
          <div class="icon-wrapper-white mr-3">
            <component :is="isEditMode ? Pencil : Plus" :size="24" />
          </div>
          <span class="text-h5 text-white">
            {{ isEditMode ? 'Edit Substance' : 'Add New Substance' }}
          </span>
          <v-spacer />
          <v-btn icon variant="text" @click="showDialog = false" :disabled="loading" color="white">
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
                <v-text-field v-model="formData.SubstanceChemical" label="Chemical Name" variant="outlined"
                  density="comfortable" required :error="!formData.SubstanceChemical" hint="Maximum 500 characters"
                  persistent-hint>
                  <template #prepend-inner>
                    <Beaker :size="20" class="text-medium-emphasis" />
                  </template>
                  <template #append-inner>
                    <v-chip size="x-small" color="error">Required</v-chip>
                  </template>
                </v-text-field>
              </v-col>

              <v-col cols="12" md="6">
                <v-text-field v-model="formData.SubstanceIdentifier" label="Identifier" variant="outlined"
                  density="comfortable" required :error="!formData.SubstanceIdentifier" hint="Maximum 100 characters"
                  persistent-hint>
                  <template #prepend-inner>
                    <Hash :size="20" class="text-medium-emphasis" />
                  </template>
                  <template #append-inner>
                    <v-chip size="x-small" color="error">Required</v-chip>
                  </template>
                </v-text-field>
              </v-col>

              <v-col cols="12" md="6">
                <v-text-field v-model="formData.SubstanceCasNo" label="CAS Number" variant="outlined"
                  density="comfortable" required :error="!formData.SubstanceCasNo" hint="Maximum 50 characters"
                  persistent-hint>
                  <template #prepend-inner>
                    <Barcode :size="20" class="text-medium-emphasis" />
                  </template>
                  <template #append-inner>
                    <v-chip size="x-small" color="error">Required</v-chip>
                  </template>
                </v-text-field>
              </v-col>

              <v-col cols="12">
                <v-divider class="my-4" />
              </v-col>

              <v-col cols="12">
                <div class="text-h6 mb-4 font-weight-medium">
                  <FileType :size="20" class="mr-2 inline-icon" />
                  Additional Details
                </div>
              </v-col>

              <v-col cols="12">
                <v-text-field v-model="formData.SubstanceThresholdLimit" label="Threshold Limit" variant="outlined"
                  density="comfortable" hint="Optional field" persistent-hint>
                  <template #prepend-inner>
                    <Gauge :size="20" class="text-medium-emphasis" />
                  </template>
                </v-text-field>
              </v-col>

              <v-col cols="12">
                <v-textarea v-model="formData.SubstanceScope" label="Scope" variant="outlined" rows="3"
                  hint="Optional field" persistent-hint>
                  <template #prepend-inner>
                    <FileText :size="20" class="text-medium-emphasis" />
                  </template>
                </v-textarea>
              </v-col>

              <v-col cols="12">
                <v-textarea v-model="formData.SubstanceExamples" label="Examples" variant="outlined" rows="3"
                  hint="Optional field" persistent-hint>
                  <template #prepend-inner>
                    <Lightbulb :size="20" class="text-medium-emphasis" />
                  </template>
                </v-textarea>
              </v-col>

              <v-col cols="12">
                <v-textarea v-model="formData.SubstanceReferences" label="References" variant="outlined" rows="3"
                  hint="Optional field" persistent-hint>
                  <template #prepend-inner>
                    <BookOpen :size="20" class="text-medium-emphasis" />
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
          <v-btn color="primary" variant="elevated" size="large" @click="saveData" :loading="loading"
            :disabled="!formData.SubstanceChemical || !formData.SubstanceIdentifier || !formData.SubstanceCasNo"
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
        <v-card-title class="d-flex align-center pa-6 bg-info">
          <div class="icon-wrapper-white mr-3">
            <Info :size="24" />
          </div>
          <span class="text-h5 text-white">Substance Details</span>
          <v-spacer />
          <v-btn icon variant="text" @click="showDetailsDialog = false" color="white">
            <X :size="20" />
          </v-btn>
        </v-card-title>

        <v-divider />

        <v-card-text class="pa-6">
          <v-list lines="two">
            <v-list-item>
              <template #prepend>
                <div class="icon-wrapper-primary">
                  <Beaker :size="20" />
                </div>
              </template>
              <v-list-item-title class="font-weight-bold">Chemical Name</v-list-item-title>
              <v-list-item-subtitle>{{ selectedSubstance.SubstanceChemical }}</v-list-item-subtitle>
            </v-list-item>

            <v-divider class="my-2" />

            <v-list-item>
              <template #prepend>
                <div class="icon-wrapper-primary">
                  <Hash :size="20" />
                </div>
              </template>
              <v-list-item-title class="font-weight-bold">Identifier</v-list-item-title>
              <v-list-item-subtitle>{{ selectedSubstance.SubstanceIdentifier }}</v-list-item-subtitle>
            </v-list-item>

            <v-divider class="my-2" />

            <v-list-item>
              <template #prepend>
                <div class="icon-wrapper-primary">
                  <Barcode :size="20" />
                </div>
              </template>
              <v-list-item-title class="font-weight-bold">CAS Number</v-list-item-title>
              <v-list-item-subtitle>{{ selectedSubstance.SubstanceCasNo }}</v-list-item-subtitle>
            </v-list-item>

            <v-divider class="my-2" />

            <v-list-item v-if="selectedSubstance.SubstanceThresholdLimit">
              <template #prepend>
                <div class="icon-wrapper-primary">
                  <Gauge :size="20" />
                </div>
              </template>
              <v-list-item-title class="font-weight-bold">Threshold Limit</v-list-item-title>
              <v-list-item-subtitle>{{ selectedSubstance.SubstanceThresholdLimit }}</v-list-item-subtitle>
            </v-list-item>

            <v-divider v-if="selectedSubstance.SubstanceThresholdLimit" class="my-2" />

            <v-list-item v-if="selectedSubstance.SubstanceScope">
              <template #prepend>
                <div class="icon-wrapper-primary">
                  <FileText :size="20" />
                </div>
              </template>
              <v-list-item-title class="font-weight-bold">Scope</v-list-item-title>
              <v-list-item-subtitle class="text-wrap">{{ selectedSubstance.SubstanceScope }}</v-list-item-subtitle>
            </v-list-item>

            <v-divider v-if="selectedSubstance.SubstanceScope" class="my-2" />

            <v-list-item v-if="selectedSubstance.SubstanceExamples">
              <template #prepend>
                <div class="icon-wrapper-primary">
                  <Lightbulb :size="20" />
                </div>
              </template>
              <v-list-item-title class="font-weight-bold">Examples</v-list-item-title>
              <v-list-item-subtitle class="text-wrap">{{ selectedSubstance.SubstanceExamples }}</v-list-item-subtitle>
            </v-list-item>

            <v-divider v-if="selectedSubstance.SubstanceExamples" class="my-2" />

            <v-list-item v-if="selectedSubstance.SubstanceReferences">
              <template #prepend>
                <div class="icon-wrapper-primary">
                  <BookOpen :size="20" />
                </div>
              </template>
              <v-list-item-title class="font-weight-bold">References</v-list-item-title>
              <v-list-item-subtitle class="text-wrap">{{ selectedSubstance.SubstanceReferences }}</v-list-item-subtitle>
            </v-list-item>
          </v-list>
        </v-card-text>

        <v-divider />

        <v-card-actions class="pa-6">
          <v-spacer />
          <v-btn color="primary" variant="text" size="large" @click="showDetailsDialog = false" class="text-none px-6">
            Close
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </div>
</template>


<style scoped>
.substance-master-container {
  background: var(--color-bg);
  min-height: 100vh;
  padding: var(--space-5) 0;
}

.header-section {
  background: var(--color-surface);
  padding: var(--space-6);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-card);
  border: 1px solid var(--color-border);
}

.main-card {
  border-radius: var(--radius-lg) !important;
  overflow: hidden;
  border: 1px solid var(--color-border) !important;
}

.stats-card {
  transition: transform var(--transition-normal), box-shadow var(--transition-normal);
  border-radius: var(--radius-lg) !important;
  border: 1px solid var(--color-border) !important;
}
.stats-card:hover { transform: translateY(-3px); box-shadow: var(--shadow-md) !important; }

.custom-table :deep(.v-data-table__tr:hover) {
  background-color: var(--color-surface-2) !important;
}

.action-buttons { display: flex; gap: var(--space-1); justify-content: center; }

.dialog-card { border-radius: var(--radius-lg) !important; }

.text-mono { font-family: var(--font-mono); font-weight: 500; }

.icon-wrapper         { display: inline-flex; align-items: center; justify-content: center; }
.icon-wrapper-white   { display: inline-flex; align-items: center; justify-content: center; color: #fff; }
.icon-wrapper-primary {
  display: inline-flex; align-items: center; justify-content: center;
  color: rgb(var(--v-theme-primary));
  background-color: rgba(var(--v-theme-primary), 0.1);
  padding: var(--space-2);
  border-radius: var(--radius-md);
}

:deep(.v-data-table) { border-radius: 0 !important; }
:deep(.v-data-table-header th) { font-weight: 600 !important; text-transform: uppercase; font-size: var(--text-xs); letter-spacing: 0.04em; }
:deep(.v-data-table__td), :deep(.v-data-table__th) { padding: var(--space-3) var(--space-4) !important; }

:deep(.v-pagination__item) { box-shadow: none !important; }
:deep(.v-pagination) { justify-content: flex-end; }

:deep(.v-overlay__content)::-webkit-scrollbar { width: 6px; }
:deep(.v-overlay__content)::-webkit-scrollbar-thumb { background: var(--color-border); border-radius: var(--radius-full); }

:deep(.v-field:hover) { box-shadow: var(--shadow-sm); }

.v-alert { animation: slideIn 0.3s ease-out; }

@keyframes slideIn {
  from { opacity: 0; transform: translateY(-8px); }
  to   { opacity: 1; transform: translateY(0); }
}

@media (max-width: 960px) {
  .substance-master-container { padding: var(--space-3) 0; }
  .header-section { padding: var(--space-4); }
}
</style>
