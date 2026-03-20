<script setup lang="ts">
interface SVHCData {
  cas_no?: string;
  ec_no?: string;
  reason_for_inclusion?: string;
  uses?: string;
  svhc_candidate?: string;
  type: 'svhc';
  [key: string]: any;
}

interface RegularData {
  chemical?: string;
  substance_identifier?: string;
  cas_no?: string;
  threshold_limit?: string;
  scope?: string;
  examples?: string;
  references?: string;
  type: 'regular';
  [key: string]: any;
}

type ChemicalData = SVHCData | RegularData;

interface SearchForm {
  searchQuery: string;
  searchType: 'all' | 'cas_no' | 'ec_no' | 'chemical_name' | 'substance_name';
}

interface SearchOptions {
  exactMatch: boolean;
  caseSensitive: boolean;
}

interface ProcessedBatchItem {
  value: string;
  type: 'cas_no' | 'ec_no' | 'name' | 'invalid';
}

interface BatchSearchData {
  rawInput: string;
  processedItems: ProcessedBatchItem[];
  skipInvalid: boolean;
}

interface BatchProcessing {
  active: boolean;
  current: number;
  total: number;
  currentItem: string;
  percentage: number;
}

interface BatchResults {
  searched: number;
  notFound: string[];
  errors: { item: string; message: string; }[];
}

// Page meta
definePageMeta({
  title: 'Chemical Database Search'
});

// Reactive data
const searchMode = ref<'single' | 'batch'>('single');
const searchForm = ref<SearchForm>({
  searchQuery: '',
  searchType: 'all'
});

const searchOptions = ref<SearchOptions>({
  exactMatch: false,
  caseSensitive: false
});

const batchSearchData = ref<BatchSearchData>({
  rawInput: '',
  processedItems: [],
  skipInvalid: true
});

const batchProcessing = ref<BatchProcessing>({
  active: false,
  current: 0,
  total: 0,
  currentItem: '',
  percentage: 0
});

const batchResults = ref<BatchResults>({
  searched: 0,
  notFound: [],
  errors: []
});

const searchResults = ref<ChemicalData[]>([]);
const loading = ref(false);
const error = ref<string | null>(null);
const hasSearched = ref(false);
const searchTime = ref('0.1');
const showOptions = ref(true); // สำหรับแสดง/ซ่อน search options

// UI State
const showAllItems = ref(false);
const showNotFound = ref(false);
const showErrors = ref(false);

// Pagination
const currentPage = ref(1);
const pageSize = ref(20);
const totalResults = ref(0);
const totalPages = computed(() => Math.ceil(totalResults.value / pageSize.value));

// Computed properties
const hasSearchCriteria = computed(() => {
  if (searchMode.value === 'single') {
    return searchForm.value.searchQuery.trim() !== '';
  } else {
    return batchSearchData.value.processedItems.length > 0;
  }
});

const searchTypeLabel = computed(() => {
  const labels = {
    'all': 'All Types',
    'cas_no': 'CAS Numbers',
    'ec_no': 'EC Numbers',
    'chemical_name': 'Chemical Names',
    'substance_name': 'Substance Names'
  };
  return labels[searchForm.value.searchType] || 'All Types';
});

const detectedSearchType = computed(() => {
  const query = searchForm.value.searchQuery.trim();
  if (!query) return null;

  if (/^[0-9]{1,7}-[0-9]{2}-[0-9]$/.test(query)) return 'cas_no';
  if (/^[0-9]{3}-[0-9]{3}-[0-9]$/.test(query)) return 'ec_no';
  if (/^[0-9\-]+$/.test(query)) return 'number_format';
  return 'name';
});

const batchStats = computed(() => {
  const items = batchSearchData.value.processedItems;
  return {
    casCount: items.filter(item => item.type === 'cas_no').length,
    ecCount: items.filter(item => item.type === 'ec_no').length,
    nameCount: items.filter(item => item.type === 'name').length,
    invalidCount: items.filter(item => item.type === 'invalid').length
  };
});

const svhcResults = computed(() => {
  return searchResults.value.filter(result => result.type === 'svhc') as SVHCData[];
});

const regularResults = computed(() => {
  return searchResults.value.filter(result => result.type === 'regular') as RegularData[];
});

const totalResultsCount = computed(() => {
  return svhcResults.value.length + regularResults.value.length;
});

// Methods
const switchSearchMode = (mode: 'single' | 'batch') => {
  searchMode.value = mode;
  clearForm();
};

const detectItemType = (value: string): 'cas_no' | 'ec_no' | 'name' | 'invalid' => {
  const trimmed = value.trim();
  if (!trimmed) return 'invalid';
  if (/^[0-9]{1,7}-[0-9]{2}-[0-9]$/.test(trimmed)) return 'cas_no';
  if (/^[0-9]{3}-[0-9]{3}-[0-9]$/.test(trimmed)) return 'ec_no';
  if (/^[0-9\-]+$/.test(trimmed)) return 'invalid';
  if (trimmed.length > 1) return 'name';
  return 'invalid';
};

const processBatchInput = () => {
  const input = batchSearchData.value.rawInput;
  if (!input.trim()) {
    batchSearchData.value.processedItems = [];
    return;
  }

  let items: string[] = [];
  const lines = input.split(/\r?\n/);

  for (const line of lines) {
    const trimmedLine = line.trim();
    if (!trimmedLine) continue;

    if (trimmedLine.includes(',')) {
      const lineItems = trimmedLine.split(/,+/).map(item => item.trim()).filter(item => item.length > 0);
      items.push(...lineItems);
    } else if (trimmedLine.includes(';')) {
      const lineItems = trimmedLine.split(/;+/).map(item => item.trim()).filter(item => item.length > 0);
      items.push(...lineItems);
    } else if (trimmedLine.includes('\t')) {
      const lineItems = trimmedLine.split(/\t+/).map(item => item.trim()).filter(item => item.length > 0);
      items.push(...lineItems);
    } else if (/\s{2,}/.test(trimmedLine)) {
      const lineItems = trimmedLine.split(/\s{2,}/).map(item => item.trim()).filter(item => item.length > 0);
      items.push(...lineItems);
    } else if (/\s+/.test(trimmedLine)) {
      const potentialItems = trimmedLine.split(/\s+/).map(item => item.trim());
      let allValid = true;
      for (const item of potentialItems) {
        const type = detectItemType(item);
        if (type === 'invalid' && item.length > 0) {
          allValid = false;
          break;
        }
      }
      if (allValid && potentialItems.length > 1) {
        items.push(...potentialItems.filter(item => item.length > 0));
      } else {
        items.push(trimmedLine);
      }
    } else {
      items.push(trimmedLine);
    }
  }

  const processedItems = items
    .map(value => {
      const cleanValue = value.replace(/[^\w\-\.]/g, '').trim();
      if (!cleanValue) return null;
      return {
        value: cleanValue,
        type: detectItemType(cleanValue)
      };
    })
    .filter(item => item !== null && item.value.length > 0);

  const uniqueItems: ProcessedBatchItem[] = [];
  const seen = new Set();

  for (const item of processedItems) {
    const key = item.value.toLowerCase();
    if (!seen.has(key)) {
      seen.add(key);
      uniqueItems.push(item);
    }
  }

  batchSearchData.value.processedItems = uniqueItems;
};

const removeBatchItem = (index: number) => {
  batchSearchData.value.processedItems.splice(index, 1);
};

const clearBatchInput = () => {
  batchSearchData.value.rawInput = '';
  batchSearchData.value.processedItems = [];
};

const searchChemicals = async () => {
  if (!hasSearchCriteria.value) {
    error.value = 'Please enter a search term or add items for batch search';
    return;
  }

  loading.value = true;
  error.value = null;
  hasSearched.value = true;
  showOptions.value = false; // ซ่อน options เมื่อเริ่ม search

  const startTime = performance.now();

  try {
    if (searchMode.value === 'single') {
      await performSingleSearch();
    } else {
      await performBatchSearch();
    }
  } catch (err: any) {
    console.error('Search error:', err);
    error.value = err.message || 'An error occurred while searching. Please try again.';
    searchResults.value = [];
    totalResults.value = 0;
  } finally {
    loading.value = false;
    batchProcessing.value.active = false;
    const endTime = performance.now();
    searchTime.value = ((endTime - startTime) / 1000).toFixed(1);
  }
};

const performSingleSearch = async () => {
  const searchParams: Record<string, any> = {
    query: searchForm.value.searchQuery,
    searchType: searchForm.value.searchType,
    exactMatch: searchOptions.value.exactMatch,
    caseSensitive: searchOptions.value.caseSensitive,
    page: currentPage.value,
    pageSize: pageSize.value
  };

  try {
    const { data, error: fetchError } = await useFetch('/api/SI25011/search', {
      method: 'GET',
      query: searchParams,
      baseURL: useRuntimeConfig().public.apiBase,
    });

    if (fetchError.value) {
      throw new Error(fetchError.value?.message || 'API request failed');
    }

    const response = data.value;
    
    // Map response to match component interface
    const svhcData = Array.isArray(response?.svhc) ? response.svhc : 
                     Array.isArray(response?.Svhc) ? response.Svhc : [];
    const regularData = Array.isArray(response?.regular) ? response.regular : 
                        Array.isArray(response?.Regular) ? response.Regular : [];

    const svhcWithType = svhcData.map((item: any) => ({
      ...item,
      type: 'svhc' as const,
      cas_no: item.CasNo || item.cas_no,
      ec_no: item.EcNo || item.ec_no,
      reason_for_inclusion: item.ReasonForInclusion || item.reason_for_inclusion,
      uses: item.Uses || item.uses,
      svhc_candidate: item.SvhcCandidate || item.svhc_candidate
    }));

    const regularWithType = regularData.map((item: any) => ({
      ...item,
      type: 'regular' as const,
      chemical: item.SubstanceChemical || item.chemical,
      substance_identifier: item.SubstanceIdentifier || item.substance_identifier,
      cas_no: item.SubstanceCasNo || item.cas_no,
      threshold_limit: item.SubstanceThresholdLimit || item.threshold_limit,
      scope: item.SubstanceScope || item.scope,
      examples: item.SubstanceExamples || item.examples,
      references: item.SubstanceReferences || item.references
    }));

    searchResults.value = [...svhcWithType, ...regularWithType];
    totalResults.value = response?.total || response?.Total || searchResults.value.length;
    currentPage.value = response?.page || response?.Page || 1;

  } catch (err: any) {
    console.error('API Error:', err);
    throw new Error(err.message || 'API request failed');
  }
};

const performBatchSearch = async () => {
  const items = batchSearchData.value.processedItems;
  const validItems = batchSearchData.value.skipInvalid
    ? items.filter(item => item.type !== 'invalid')
    : items;

  if (validItems.length === 0) {
    throw new Error('No valid items to search');
  }

  batchProcessing.value = {
    active: true,
    current: 0,
    total: validItems.length,
    currentItem: '',
    percentage: 0
  };

  batchResults.value = {
    searched: 0,
    notFound: [],
    errors: []
  };

  try {
    const batchRequest = {
      items: validItems.map(item => ({
        value: item.value,
        type: item.type === 'cas_no' ? 'cas_no' :
              item.type === 'ec_no' ? 'ec_no' :
              item.type === 'name' ? 'chemical_name' : 'all'
      })),
      skipInvalid: batchSearchData.value.skipInvalid,
      exactMatch: searchOptions.value.exactMatch,
      caseSensitive: searchOptions.value.caseSensitive
    };

    const { data, error: fetchError } = await useFetch('/api/SI25011/batch-search', {
      method: 'POST',
      baseURL: useRuntimeConfig().public.apiBase,
      body: batchRequest,
      headers: {
        'Content-Type': 'application/json'
      }
    });

    if (fetchError.value) {
      throw new Error(fetchError.value?.message || 'Batch search failed');
    }

    const response = data.value;
    
    if (response && response.Results) {
      const allResults = [];

      for (const result of response.Results) {
        if (result.QaSubstances && result.QaSubstances.length > 0) {
          const svhcWithType = result.QaSubstances.map((item: any) => ({
            ...item,
            type: 'svhc' as const,
            cas_no: item.CasNo || item.cas_no,
            ec_no: item.EcNo || item.ec_no,
            reason_for_inclusion: item.ReasonForInclusion || item.reason_for_inclusion,
            uses: item.Uses || item.uses,
            svhc_candidate: item.SvhcCandidate || item.svhc_candidate
          }));
          allResults.push(...svhcWithType);
        }

        if (result.RegularSubstands && result.RegularSubstands.length > 0) {
          const regularWithType = result.RegularSubstands.map((item: any) => ({
            ...item,
            type: 'regular' as const,
            chemical: item.SubstanceChemical || item.chemical,
            substance_identifier: item.SubstanceIdentifier || item.substance_identifier,
            cas_no: item.SubstanceCasNo || item.cas_no,
            threshold_limit: item.SubstanceThresholdLimit || item.threshold_limit,
            scope: item.SubstanceScope || item.scope,
            examples: item.SubstanceExamples || item.examples,
            references: item.SubstanceReferences || item.references
          }));
          allResults.push(...regularWithType);
        }
      }

      const uniqueResults = allResults.filter((result, index, arr) => {
        if (result.type === 'svhc') {
          return !arr.slice(0, index).some(r =>
            r.type === 'svhc' && r.cas_no === result.cas_no && r.ec_no === result.ec_no
          );
        } else {
          return !arr.slice(0, index).some(r =>
            r.type === 'regular' && r.cas_no === result.cas_no && r.chemical === result.chemical
          );
        }
      });

      searchResults.value = uniqueResults;
      totalResults.value = uniqueResults.length;
      currentPage.value = 1;

      if (response.Summary) {
        batchResults.value = {
          searched: response.Summary.TotalSearched || validItems.length,
          notFound: response.Summary.NotFound || [],
          errors: response.Summary.Errors?.map((err: any) => ({
            item: err.Item || err.item,
            message: err.Message || err.message
          })) || []
        };
      }

      batchProcessing.value.current = validItems.length;
      batchProcessing.value.percentage = 100;

    } else {
      searchResults.value = [];
      totalResults.value = 0;
      batchResults.value.searched = validItems.length;
      batchResults.value.notFound = validItems.map(item => item.value);
    }

  } catch (err: any) {
    console.error('Batch search error:', err);
    throw new Error(err.message || 'Batch search failed');
  }
};

const clearForm = () => {
  searchForm.value = {
    searchQuery: '',
    searchType: 'all'
  };

  searchOptions.value = {
    exactMatch: false,
    caseSensitive: false
  };

  batchSearchData.value = {
    rawInput: '',
    processedItems: [],
    skipInvalid: true
  };

  batchResults.value = {
    searched: 0,
    notFound: [],
    errors: []
  };

  searchResults.value = [];
  error.value = null;
  hasSearched.value = false;
  currentPage.value = 1;
  totalResults.value = 0;
  showAllItems.value = false;
  showNotFound.value = false;
  showErrors.value = false;
  showOptions.value = true; // แสดง options อีกครั้ง
};

const newSearch = () => {
  showOptions.value = true;
  hasSearched.value = false;
  searchResults.value = [];
  error.value = null;
};

const previousPage = () => {
  if (currentPage.value > 1) {
    currentPage.value--;
    searchChemicals();
  }
};

const nextPage = () => {
  if (currentPage.value < totalPages.value) {
    currentPage.value++;
    searchChemicals();
  }
};

// Watch for search query changes
watch(() => searchForm.value.searchQuery, (newValue) => {
  if (error.value && newValue.trim()) {
    error.value = null;
  }
});

watch(() => batchSearchData.value.rawInput, (newValue) => {
  if (error.value && newValue.trim()) {
    error.value = null;
  }
});

// Helper function to get icon name for batch items
const getItemIconName = (itemType: string) => {
  switch (itemType) {
    case 'cas_no': return 'tabler-hash'
    case 'ec_no': return 'tabler-bookmark'
    case 'name': return 'tabler-flask'
    case 'invalid': return 'tabler-alert-triangle'
    default: return 'tabler-help'
  }
};

const getDetectionBadgeClass = (type: string | null) => {
  switch (type) {
    case 'cas_no': return 'cas-detected';
    case 'ec_no': return 'ec-detected';
    case 'name': return 'name-detected';
    case 'number_format': return 'invalid-detected';
    default: return '';
  }
};

const getDetectionLabel = (type: string | null) => {
  switch (type) {
    case 'cas_no': return 'CAS Number';
    case 'ec_no': return 'EC Number';
    case 'name': return 'Chemical Name';
    case 'number_format': return 'Invalid Format';
    default: return '';
  }
};
</script>

<template>
  <div class="min-vh-100 bg-gradient-modern">
    <!-- Background Animation -->
    <div class="floating-shapes">
      <div class="shape shape-1"></div>
      <div class="shape shape-2"></div>
      <div class="shape shape-3"></div>
      <div class="shape shape-4"></div>
    </div>

    <div class="container-fluid py-5 position-relative">
      <!-- Hero Header -->
      <div class="row justify-content-center mb-4">
        <div class="col-xl-10">
          <div class="hero-card glass-effect text-center p-4">
            <h1 class="hero-title mb-3">
              <span class="gradient-text">Chemical Database</span>
              <span class="text-dark">Search</span>
            </h1>
            <p class="hero-subtitle mb-0">
              Advanced search for chemical information with AI-powered detection & Batch Processing
            </p>
            <div class="hero-features mt-3">
              <span class="feature-badge">CAS Numbers</span>
              <span class="feature-badge">EC Numbers</span>
              <span class="feature-badge">Chemical Names</span>
              <span class="feature-badge">SVHC Database</span>
              <span class="feature-badge">Batch Search</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Advanced Search Panel -->
      <div class="row justify-content-center">
        <div class="col-xl-10">
          <div class="search-panel glass-effect p-4 p-lg-5">
            
            <!-- New Search Button (แสดงเมื่อมีผลลัพธ์และ options ถูกซ่อน) -->
            <div v-if="hasSearched && !showOptions" class="text-center mb-4">
              <button @click="newSearch" class="btn-new-search">
                <v-icon icon="tabler-search" size="20" class="me-2"></v-icon>
                New Search
              </button>
            </div>

            <!-- Search Form (แสดงเมื่อ showOptions เป็น true) -->
            <form v-show="showOptions" @submit.prevent="searchChemicals">

              <!-- Search Mode Tabs -->
              <div class="search-mode-tabs mb-4">
                <div class="tab-container">
                  <button type="button" class="tab-btn" :class="{ active: searchMode === 'single' }"
                    @click="switchSearchMode('single')">
                    <v-icon icon="tabler-search" size="18"></v-icon>
                    Single Search
                  </button>
                  <button type="button" class="tab-btn" :class="{ active: searchMode === 'batch' }"
                    @click="switchSearchMode('batch')">
                    <v-icon icon="tabler-list" size="18"></v-icon>
                    Batch Search
                  </button>
                </div>
              </div>

              <!-- Single Search Mode -->
              <div v-show="searchMode === 'single'" class="single-search-section">
                <!-- Smart Search Input -->
                <div class="search-input-container mb-4">
                  <div class="search-input-wrapper">
                    <div class="search-icon">
                      <v-icon icon="tabler-search" size="18"></v-icon>
                    </div>
                    <input v-model="searchForm.searchQuery" type="text" class="search-input"
                      placeholder="Enter CAS No., EC No., Chemical Name, or Substance..." autocomplete="off" />
                    <div v-if="searchForm.searchQuery" class="clear-btn" @click="searchForm.searchQuery = ''">
                      <v-icon icon="tabler-circle-x" size="18"></v-icon>
                    </div>
                  </div>
                  <!-- Auto Detection Display -->
                  <div v-if="searchForm.searchQuery && searchForm.searchType === 'all'" class="auto-detection">
                    <div class="detection-indicator">
                      <span class="detection-label">AI Detection:</span>
                      <div class="detection-result">
                        <span v-if="detectedSearchType" class="detection-badge" :class="getDetectionBadgeClass(detectedSearchType)">
                          <v-icon icon="tabler-cpu" size="18" class="me-1"></v-icon>{{ getDetectionLabel(detectedSearchType) }}
                        </span>
                      </div>
                    </div>
                  </div>
                </div>

                <!-- Search Type Selector -->
                <div class="search-types-section mb-4">
                  <h6 class="section-title mb-3">
                    <v-icon icon="tabler-adjustments" size="18" class="me-2"></v-icon>Search Type
                  </h6>
                  <div class="search-types-grid">
                    <label class="search-type-card" :class="{ active: searchForm.searchType === 'all' }">
                      <input v-model="searchForm.searchType" type="radio" value="all" class="d-none">
                      <div class="card-content">
                        <div class="card-icon all-types">
                          <v-icon icon="tabler-wand" size="18"></v-icon>
                        </div>
                        <div class="card-info">
                          <div class="card-title">Smart Search</div>
                          <div class="card-desc">Auto-detect type</div>
                        </div>
                      </div>
                    </label>

                    <label class="search-type-card" :class="{ active: searchForm.searchType === 'cas_no' }">
                      <input v-model="searchForm.searchType" type="radio" value="cas_no" class="d-none">
                      <div class="card-content">
                        <div class="card-icon cas-type">
                          <v-icon icon="tabler-hash" size="18"></v-icon>
                        </div>
                        <div class="card-info">
                          <div class="card-title">CAS Number</div>
                          <div class="card-desc">123-45-6</div>
                        </div>
                      </div>
                    </label>

                    <label class="search-type-card" :class="{ active: searchForm.searchType === 'ec_no' }">
                      <input v-model="searchForm.searchType" type="radio" value="ec_no" class="d-none">
                      <div class="card-content">
                        <div class="card-icon ec-type">
                          <v-icon icon="tabler-bookmark" size="18"></v-icon>
                        </div>
                        <div class="card-info">
                          <div class="card-title">EC Number</div>
                          <div class="card-desc">200-123-4</div>
                        </div>
                      </div>
                    </label>

                    <label class="search-type-card" :class="{ active: searchForm.searchType === 'chemical_name' }">
                      <input v-model="searchForm.searchType" type="radio" value="chemical_name" class="d-none">
                      <div class="card-content">
                        <div class="card-icon chemical-type">
                          <v-icon icon="tabler-flask" size="18"></v-icon>
                        </div>
                        <div class="card-info">
                          <div class="card-title">Chemical Name</div>
                          <div class="card-desc">Acetone</div>
                        </div>
                      </div>
                    </label>

                    <label class="search-type-card" :class="{ active: searchForm.searchType === 'substance_name' }">
                      <input v-model="searchForm.searchType" type="radio" value="substance_name" class="d-none">
                      <div class="card-content">
                        <div class="card-icon substance-type">
                          <v-icon icon="tabler-layers" size="18"></v-icon>
                        </div>
                        <div class="card-info">
                          <div class="card-title">Substance Name</div>
                          <div class="card-desc">Propanone</div>
                        </div>
                      </div>
                    </label>
                  </div>
                </div>
              </div>

              <!-- Batch Search Mode -->
              <div v-show="searchMode === 'batch'" class="batch-search-section">
                <!-- Input Section -->
                <div class="batch-input-section mb-4">
                  <div class="input-label-section mb-3">
                    <label class="input-main-label">
                      <v-icon icon="tabler-clipboard-data" size="18" class="me-2"></v-icon>
                      Paste Your Chemical Data
                    </label>
                    <div class="format-examples">
                      <span class="format-example">CAS: 67-64-1</span>
                      <span class="format-example">EC: 200-662-2</span>
                      <span class="format-example">Name: Acetone</span>
                    </div>
                  </div>

                  <div class="textarea-wrapper">
                    <textarea v-model="batchSearchData.rawInput" @input="processBatchInput" class="batch-textarea"
                      placeholder="Examples:

67-64-1
200-662-2  
Acetone
Benzene
81-15-2

Or paste from Excel/CSV:
67-64-1, 200-662-2, Acetone
81-15-2 | Benzene | Toluene

We'll detect the format automatically!" rows="8"></textarea>

                    <div v-if="batchSearchData.rawInput.trim()" class="processing-indicator">
                      <div class="indicator-content">
                        <div class="pulse-dot"></div>
                        <span>Processing {{ batchSearchData.processedItems.length }} items...</span>
                      </div>
                    </div>
                  </div>
                </div>

                <!-- Results Preview -->
                <div v-if="batchSearchData.processedItems.length > 0" class="batch-preview-section">
                  <!-- Summary Stats -->
                  <div class="preview-summary mb-3">
                    <div class="summary-header">
                      <h6 class="summary-title">
                        <v-icon icon="tabler-check-circle" size="18" class="me-2"></v-icon>
                        Items Ready for Search
                      </h6>
                      <span class="total-count">{{ batchSearchData.processedItems.length }} total</span>
                    </div>

                    <div class="type-breakdown">
                      <div v-if="batchStats.casCount > 0" class="type-stat cas">
                        <span class="count">{{ batchStats.casCount }}</span>
                        <span class="label">CAS Numbers</span>
                      </div>
                      <div v-if="batchStats.ecCount > 0" class="type-stat ec">
                        <span class="count">{{ batchStats.ecCount }}</span>
                        <span class="label">EC Numbers</span>
                      </div>
                      <div v-if="batchStats.nameCount > 0" class="type-stat names">
                        <span class="count">{{ batchStats.nameCount }}</span>
                        <span class="label">Chemical Names</span>
                      </div>
                      <div v-if="batchStats.invalidCount > 0" class="type-stat invalid">
                        <span class="count">{{ batchStats.invalidCount }}</span>
                        <span class="label">Invalid/Unrecognized</span>
                      </div>
                    </div>
                  </div>

                  <!-- Items Grid -->
                  <div class="items-grid">
                    <div v-for="(item, index) in batchSearchData.processedItems.slice(0, showAllItems ? undefined : 12)"
                      :key="index" class="batch-item" :class="[item.type, { 'invalid': item.type === 'invalid' }]">
                      <div class="item-content">
                        <div class="item-type-badge" :class="item.type">
                          <v-icon :icon="getItemIconName(item.type)" size="18" />
                        </div>
                        <div class="item-details">
                          <div class="item-value">{{ item.value }}</div>
                          <div class="item-type-label">
                            {{ item.type === 'cas_no' ? 'CAS Number' :
                              item.type === 'ec_no' ? 'EC Number' :
                              item.type === 'name' ? 'Chemical Name' : 'Invalid Format' }}
                          </div>
                        </div>
                        <button @click="removeBatchItem(index)" class="remove-item">
                          <v-icon icon="tabler-x" size="18"></v-icon>
                        </button>
                      </div>
                    </div>

                    <!-- Show More Button -->
                    <div v-if="batchSearchData.processedItems.length > 12 && !showAllItems" class="show-more-item">
                      <button @click="showAllItems = true" class="show-more-btn">
                        <v-icon icon="tabler-plus" size="18" class="me-2"></v-icon>
                        Show {{ batchSearchData.processedItems.length - 12 }} more items
                      </button>
                    </div>

                    <!-- Show Less Button -->
                    <div v-if="showAllItems && batchSearchData.processedItems.length > 12" class="show-less-item">
                      <button @click="showAllItems = false" class="show-less-btn">
                        <v-icon icon="tabler-minus" size="18" class="me-2"></v-icon>
                        Show less
                      </button>
                    </div>
                  </div>

                  <!-- Invalid Items Warning -->
                  <div v-if="batchStats.invalidCount > 0" class="invalid-warning">
                    <div class="warning-content">
                      <v-icon icon="tabler-alert-triangle" size="18" class="me-2"></v-icon>
                      <span>
                        {{ batchStats.invalidCount }} item{{ batchStats.invalidCount > 1 ? 's' : '' }} couldn't be recognized.
                        <span v-if="batchSearchData.skipInvalid">They will be skipped during search.</span>
                        <span v-else>Please check the format or they may return no results.</span>
                      </span>
                    </div>
                  </div>

                  <!-- Ready to Search Indicator -->
                  <div v-if="batchStats.casCount + batchStats.ecCount + batchStats.nameCount > 0" class="ready-indicator">
                    <div class="ready-content">
                      <div class="ready-icon">
                        <v-icon icon="tabler-rocket" size="18"></v-icon>
                      </div>
                      <div class="ready-text">
                        <div class="ready-title">Ready to Search!</div>
                        <div class="ready-subtitle">
                          {{ batchStats.casCount + batchStats.ecCount + batchStats.nameCount }} valid items detected
                        </div>
                      </div>
                    </div>
                  </div>
                </div>

                <!-- Empty State -->
                <div v-else class="batch-empty-state">
                  <div class="empty-content">
                    <div class="empty-icon">
                      <v-icon icon="tabler-clipboard" size="48"></v-icon>
                    </div>
                    <h6 class="empty-title">No Data Added Yet</h6>
                    <p class="empty-text">
                      Paste or type your chemical identifiers above to get started.
                      We support CAS numbers, EC numbers, and chemical names.
                    </p>
                  </div>
                </div>
              </div>

              <!-- Advanced Options -->
              <div class="advanced-options mb-4">
                <div class="row align-items-center">
                  <div class="col-md-6">
                    <div class="options-group">
                      <label class="modern-switch">
                        <input v-model="searchOptions.exactMatch" type="checkbox">
                        <span class="switch-slider"></span>
                        <span class="switch-label">
                          <v-icon icon="tabler-target" size="18" class="me-1"></v-icon>Exact Match
                        </span>
                      </label>

                      <label class="modern-switch">
                        <input v-model="searchOptions.caseSensitive" type="checkbox">
                        <span class="switch-slider"></span>
                        <span class="switch-label">
                          <v-icon icon="tabler-letter-case" size="18" class="me-1"></v-icon>Case Sensitive
                        </span>
                      </label>

                      <label v-if="searchMode === 'batch'" class="modern-switch">
                        <input v-model="batchSearchData.skipInvalid" type="checkbox">
                        <span class="switch-slider"></span>
                        <span class="switch-label">
                          <v-icon icon="tabler-player-skip-forward" size="18" class="me-1"></v-icon>Skip Invalid Items
                        </span>
                      </label>
                    </div>
                  </div>
                  <div class="col-md-6">
                    <div class="search-stats text-md-end">
                      <div class="stat-item">
                        <v-icon icon="tabler-database" size="18" class="me-1"></v-icon>
                        <span>150,000+ Chemicals</span>
                      </div>
                      <div class="stat-item">
                        <v-icon icon="tabler-shield-exclamation" size="18" class="me-1"></v-icon>
                        <span>SVHC Updated</span>
                      </div>
                      <div v-if="searchMode === 'batch'" class="stat-item">
                        <v-icon icon="tabler-bolt" size="18" class="me-1"></v-icon>
                        <span>Batch Processing</span>
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              <!-- Action Buttons -->
              <div class="action-buttons">
                <button type="submit" :disabled="loading || !hasSearchCriteria" class="btn-primary-modern">
                  <span v-if="loading" class="btn-loading">
                    <span class="spinner"></span>
                    <span v-if="searchMode === 'batch'">Processing {{ batchSearchData.processedItems.length }} items...</span>
                    <span v-else>Searching...</span>
                  </span>
                  <span v-else class="btn-content">
                    <v-icon :icon="searchMode === 'batch' ? 'tabler-list-check' : 'tabler-search'" size="18" class="me-2"></v-icon>
                    <span v-if="searchMode === 'batch'">Search {{ batchSearchData.processedItems.length }} Items</span>
                    <span v-else>Search {{ searchTypeLabel }}</span>
                  </span>
                </button>

                <button type="button" @click="clearForm" class="btn-secondary-modern">
                  <v-icon icon="tabler-trash" size="18" class="me-2"></v-icon>
                  <span>Clear All</span>
                </button>
              </div>
            </form>
          </div>
        </div>
      </div>

      <!-- Batch Progress -->
      <div v-if="batchProcessing.active" class="row justify-content-center mt-4">
        <div class="col-xl-10">
          <div class="batch-progress glass-effect">
            <div class="progress-header">
              <h6 class="progress-title">
                <v-icon icon="tabler-loader" size="18" class="me-2 spin-animation"></v-icon>
                Processing Batch Search
              </h6>
              <div class="progress-stats">
                {{ batchProcessing.current }} of {{ batchProcessing.total }}
              </div>
            </div>
            <div class="progress-bar-container">
              <div class="progress-bar" :style="{ width: batchProcessing.percentage + '%' }"></div>
            </div>
            <div class="progress-details">
              <span class="current-item">{{ batchProcessing.currentItem }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Error Alert -->
      <div v-if="error" class="row justify-content-center mt-4">
        <div class="col-xl-10">
          <div class="error-alert glass-effect">
            <div class="error-content">
              <div class="error-icon">
                <v-icon icon="tabler-alert-triangle" size="24"></v-icon>
              </div>
              <div class="error-text">
                <div class="error-title">No Detect</div>
                <div class="error-message">{{ error }}</div>
              </div>
              <button class="error-close" @click="error = null">
                <v-icon icon="tabler-x" size="18"></v-icon>
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Result Box -->
      <div class="result-box" v-if="hasSearched && !loading">
        <button :class="['result-button', searchResults.length > 0 ? 'detect-button' : 'not-detect-button']">
          {{ searchResults.length > 0 ? 'Detect' : 'Not Detect' }}
        </button>
      </div>

      <!-- Results Section -->
      <div v-if="searchResults.length > 0 && hasSearched" class="results-section mt-5">
        <!-- Results Summary -->
        <div class="row justify-content-center mb-4">
          <div class="col-xl-10">
            <div class="results-summary glass-effect">
              <div class="summary-content">
                <div class="summary-stats">
                  <div class="stat-card svhc-stat">
                    <div class="stat-number">{{ svhcResults.length }}</div>
                    <div class="stat-label">SVHC Found</div>
                  </div>
                  <div class="stat-card regular-stat">
                    <div class="stat-number">{{ regularResults.length }}</div>
                    <div class="stat-label">Regular Substances</div>
                  </div>
                  <div class="stat-card total-stat">
                    <div class="stat-number">{{ totalResultsCount }}</div>
                    <div class="stat-label">Total Results</div>
                  </div>
                  <div v-if="searchMode === 'batch'" class="stat-card batch-stat">
                    <div class="stat-number">{{ batchResults.searched }}</div>
                    <div class="stat-label">Items Searched</div>
                  </div>
                </div>
                <div class="search-info">
                  <div class="search-query">
                    <v-icon :icon="searchMode === 'batch' ? 'tabler-list' : 'tabler-search'" size="18" class="me-2"></v-icon>
                    <span v-if="searchMode === 'batch'">Batch Search ({{ batchSearchData.processedItems.length }} items)</span>
                    <span v-else>"{{ searchForm.searchQuery }}"</span>
                  </div>
                  <div class="search-time">
                    <v-icon icon="tabler-clock" size="18" class="me-1"></v-icon>
                    <span>{{ searchTime }}s</span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- SVHC Results -->
        <div v-if="svhcResults.length > 0" class="row justify-content-center mb-4">
          <div class="col-xl-10">
            <div class="results-card svhc-results glass-effect">
              <div class="results-header svhc-header">
                <div class="header-content">
                  <div class="header-icon">
                    <v-icon icon="tabler-alert-triangle" size="24"></v-icon>
                  </div>
                  <div class="header-info">
                    <h5 class="header-title">SVHC Results</h5>
                    <p class="header-subtitle">Substances of Very High Concern ({{ svhcResults.length }} found)</p>
                  </div>
                </div>
                <div class="header-badge danger-badge">
                  <v-icon icon="tabler-shield-exclamation" size="18" class="me-1"></v-icon>
                  High Risk
                </div>
              </div>

              <div class="results-content">
                <div class="table-container">
                  <table class="modern-table improved-table">
                    <thead>
                      <tr>
                        <th><v-icon icon="tabler-hash" size="18" class="me-1"></v-icon>CAS No.</th>
                        <th><v-icon icon="tabler-bookmark" size="18" class="me-1"></v-icon>EC No.</th>
                        <th><v-icon icon="tabler-info-circle" size="18" class="me-1"></v-icon>Reason</th>
                        <th><v-icon icon="tabler-tools" size="18" class="me-1"></v-icon>Uses</th>
                        <th><v-icon icon="tabler-shield" size="18" class="me-1"></v-icon>Status</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr v-for="(result, index) in svhcResults" :key="`svhc-${index}`" class="table-row improved-row">
                        <td>
                          <div class="cas-number improved-cas">{{ result.cas_no || '-' }}</div>
                        </td>
                        <td>
                          <div class="ec-number improved-ec">{{ result.ec_no || '-' }}</div>
                        </td>
                        <td>
                          <div class="reason-text improved-text" :title="result.reason_for_inclusion">
                            {{ result.reason_for_inclusion || '-' }}
                          </div>
                        </td>
                        <td>
                          <div class="uses-text improved-text" :title="result.uses">
                            {{ result.uses || '-' }}
                          </div>
                        </td>
                        <td>
                          <span v-if="result.svhc_candidate" class="status-badge improved-badge"
                            :class="result.svhc_candidate.toLowerCase() === 'yes' ? 'status-danger' : 'status-info'">
                            {{ result.svhc_candidate }}
                          </span>
                          <span v-else class="text-muted improved-muted">-</span>
                        </td>
                      </tr>
                    </tbody>
                  </table>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Regular Results -->
        <div v-if="regularResults.length > 0" class="row justify-content-center mb-4">
          <div class="col-xl-10">
            <div class="results-card regular-results glass-effect">
              <div class="results-header regular-header">
                <div class="header-content">
                  <div class="header-icon">
                    <v-icon icon="tabler-flask" size="24"></v-icon>
                  </div>
                  <div class="header-info">
                    <h5 class="header-title">Regular Substances</h5>
                    <p class="header-subtitle">Standard Chemical Database ({{ regularResults.length }} found)</p>
                  </div>
                </div>
                <div class="header-badge success-badge">
                  <v-icon icon="tabler-check-circle" size="18" class="me-1"></v-icon>
                  Standard
                </div>
              </div>

              <div class="results-content">
                <div class="table-container">
                  <table class="modern-table improved-table">
                    <thead>
                      <tr>
                        <th><v-icon icon="tabler-flask" size="18" class="me-1"></v-icon>Chemical</th>
                        <th><v-icon icon="tabler-fingerprint" size="18" class="me-1"></v-icon>Identifier</th>
                        <th><v-icon icon="tabler-hash" size="18" class="me-1"></v-icon>CAS No.</th>
                        <th><v-icon icon="tabler-gauge" size="18" class="me-1"></v-icon>Limit</th>
                        <th><v-icon icon="tabler-target" size="18" class="me-1"></v-icon>Scope</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr v-for="(result, index) in regularResults" :key="`regular-${index}`" class="table-row improved-row">
                        <td>
                          <div class="chemical-name improved-chemical">{{ result.chemical || '-' }}</div>
                        </td>
                        <td>
                          <div class="identifier-text improved-identifier">{{ result.substance_identifier || '-' }}</div>
                        </td>
                        <td>
                          <div class="cas-number improved-cas">{{ result.cas_no || '-' }}</div>
                        </td>
                        <td>
                          <span class="threshold-badge improved-threshold">{{ result.threshold_limit || '-' }}</span>
                        </td>
                        <td>
                          <div class="scope-text improved-text" :title="result.scope">{{ result.scope || '-' }}</div>
                        </td>
                      </tr>
                    </tbody>
                  </table>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Batch Results Summary -->
        <div v-if="searchMode === 'batch' && (batchResults.notFound.length > 0 || batchResults.errors.length > 0)"
          class="row justify-content-center mb-4">
          <div class="col-xl-10">
            <div class="batch-summary glass-effect">
              <h6 class="batch-summary-title">
                <v-icon icon="tabler-info-circle" size="18" class="me-2"></v-icon>
                Batch Processing Summary
              </h6>

              <!-- Not Found Items -->
              <div v-if="batchResults.notFound.length > 0" class="not-found-section">
                <div class="section-header">
                  <span class="section-title">Items Not Found ({{ batchResults.notFound.length }})</span>
                  <button type="button" @click="showNotFound = !showNotFound" class="toggle-section-btn">
                    <v-icon :icon="showNotFound ? 'tabler-chevron-up' : 'tabler-chevron-down'" size="18"></v-icon>
                  </button>
                </div>
                <div v-show="showNotFound" class="not-found-items">
                  <div v-for="item in batchResults.notFound" :key="item" class="not-found-item">
                    {{ item }}
                  </div>
                </div>
              </div>

              <!-- Error Items -->
              <div v-if="batchResults.errors.length > 0" class="error-section">
                <div class="section-header">
                  <span class="section-title">Errors ({{ batchResults.errors.length }})</span>
                  <button type="button" @click="showErrors = !showErrors" class="toggle-section-btn">
                    <v-icon :icon="showErrors ? 'tabler-chevron-up' : 'tabler-chevron-down'" size="18"></v-icon>
                  </button>
                </div>
                <div v-show="showErrors" class="error-items">
                  <div v-for="error in batchResults.errors" :key="error.item" class="error-item">
                    <strong>{{ error.item }}:</strong> {{ error.message }}
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Pagination -->
        <div v-if="totalPages > 1" class="row justify-content-center">
          <div class="col-xl-10">
            <div class="pagination-card glass-effect">
              <div class="pagination-content">
                <div class="pagination-info">
                  <span>Showing {{ (currentPage - 1) * pageSize + 1 }}-{{ Math.min(currentPage * pageSize, totalResultsCount) }} of {{ totalResultsCount }}</span>
                </div>
                <div class="pagination-controls">
                  <button @click="previousPage" :disabled="currentPage === 1" class="pagination-btn">
                    <v-icon icon="tabler-chevron-left" size="18"></v-icon>
                    Previous
                  </button>
                  <span class="current-page">{{ currentPage }}</span>
                  <button @click="nextPage" :disabled="currentPage === totalPages" class="pagination-btn">
                    Next
                    <v-icon icon="tabler-chevron-right" size="18"></v-icon>
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

    </div>
  </div>
</template>

<style scoped>
/* Modern Light Background */
.bg-gradient-modern {
  background: linear-gradient(135deg, #f8fafc 0%, #e2e8f0 100%);
  position: relative;
  overflow-x: hidden;
  min-height: 100vh;
}

/* Floating Background Shapes */
.floating-shapes {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  overflow: hidden;
  z-index: 0;
}

.shape {
  position: absolute;
  background: rgba(102, 126, 234, 0.08);
  border-radius: 50%;
  animation: float 20s infinite linear;
}

.shape-1 {
  width: 80px;
  height: 80px;
  left: 10%;
  animation-delay: 0s;
}

.shape-2 {
  width: 120px;
  height: 120px;
  left: 80%;
  animation-delay: 5s;
}

.shape-3 {
  width: 60px;
  height: 60px;
  left: 60%;
  animation-delay: 10s;
}

.shape-4 {
  width: 100px;
  height: 100px;
  left: 30%;
  animation-delay: 15s;
}

@keyframes float {
  0% {
    transform: translateY(100vh) rotate(0deg);
  }
  100% {
    transform: translateY(-100px) rotate(360deg);
  }
}

/* Glass Effect */
.glass-effect {
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(20px);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 20px;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.1);
  position: relative;
  z-index: 1;
}

/* Hero Section */
.hero-card {
  padding: 2rem !important;
  text-align: center;
}

.hero-title {
  font-size: 2.5rem;
  font-weight: 800;
  margin-bottom: 1rem;
  color: #1e293b;
}

.gradient-text {
  background: linear-gradient(45deg, #ffd700, #ff6b6b);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.hero-subtitle {
  font-size: 1rem;
  color: #64748b;
  font-weight: 400;
}

.hero-features {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
  justify-content: center;
}

.feature-badge {
  padding: 0.4rem 0.8rem;
  background: rgba(102, 126, 234, 0.1);
  border: 1px solid rgba(102, 126, 234, 0.2);
  border-radius: 25px;
  color: #667eea;
  font-size: 0.8rem;
  font-weight: 500;
}

/* New Search Button */
.btn-new-search {
  padding: 1.25rem 3rem;
  background: linear-gradient(135deg, #667eea, #764ba2);
  border: none;
  border-radius: 50px;
  color: white;
  font-size: 1.2rem;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.3s ease;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 0.75rem;
  box-shadow: 0 15px 35px rgba(102, 126, 234, 0.3);
  animation: pulse-subtle 2s ease-in-out infinite;
}

.btn-new-search:hover {
  transform: translateY(-3px) scale(1.02);
  box-shadow: 0 20px 45px rgba(102, 126, 234, 0.4);
}

@keyframes pulse-subtle {
  0%, 100% {
    box-shadow: 0 15px 35px rgba(102, 126, 234, 0.3);
  }
  50% {
    box-shadow: 0 15px 35px rgba(102, 126, 234, 0.5);
  }
}

/* Search Panel */
.search-panel {
  padding: 2.5rem !important;
  background: white;
}

/* Modern Search Input */
.search-input-container {
  position: relative;
}

.search-input-wrapper {
  position: relative;
  display: flex;
  align-items: center;
}

.search-input {
  width: 100%;
  padding: 1.5rem 4rem 1.5rem 4rem;
  font-size: 1.1rem;
  border: 2px solid #e2e8f0;
  border-radius: 50px;
  background: #ffffff;
  color: #1e293b;
  transition: all 0.3s ease;
  font-weight: 500;
}

.search-input:focus {
  outline: none;
  border-color: #667eea;
  box-shadow: 0 0 20px rgba(102, 126, 234, 0.15);
  background: #ffffff;
}

.search-icon {
  position: absolute;
  left: 1.5rem;
  color: #64748b;
  font-size: 1.2rem;
  z-index: 2;
}

.clear-btn {
  position: absolute;
  right: 1.5rem;
  color: #94a3b8;
  cursor: pointer;
  font-size: 1.1rem;
  z-index: 2;
  transition: color 0.3s ease;
}

.clear-btn:hover {
  color: #ef4444;
}

/* แก้ไข switch-slider */
.modern-switch {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  cursor: pointer;
  color: #374151;
  font-size: 0.9rem;
}

.modern-switch input {
  display: none;
}

.switch-slider {
  position: relative;
  /* เพิ่ม position relative */
  width: 50px;
  height: 26px;
  background: #cbd5e1;
  border-radius: 13px;
  /* เพิ่ม border-radius */
  transition: all 0.3s ease;
}

.switch-slider::before {
  content: '';
  position: absolute;
  width: 20px;
  height: 20px;
  border-radius: 50%;
  background: white;
  top: 3px;
  left: 3px;
  transition: all 0.3s ease;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
}

.modern-switch input:checked+.switch-slider {
  background: #667eea;
}

.modern-switch input:checked+.switch-slider::before {
  transform: translateX(24px);
}

.switch-label {
  display: flex;
  align-items: center;
  gap: 0.25rem;
}

/* Auto Detection */
.auto-detection {
  position: absolute;
  top: 100%;
  right: 0;
  margin-top: 0.5rem;
  z-index: 10;
}

.detection-indicator {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.detection-label {
  color: #64748b;
  font-size: 0.9rem;
  font-weight: 500;
}

.detection-badge {
  padding: 0.25rem 0.75rem;
  border-radius: 15px;
  font-size: 0.8rem;
  font-weight: 600;
  animation: slideInRight 0.3s ease;
}

.cas-detected {
  background: #22c55e;
  /* color: white; */
}

.ec-detected {
  background: #06b6d4;
  /* color: white; */
}

.name-detected {
  background: #8b5cf6;
  /* color: white; */
}

.invalid-detected {
  background: #f59e0b;
  /* color: white; */
}

@keyframes slideInRight {
  from {
    transform: translateX(20px);
    opacity: 0;
  }

  to {
    transform: translateX(0);
    opacity: 1;
  }
}

.examples-label {
  color: #64748b;
  font-size: 0.9rem;
  margin-bottom: 0.5rem;
  display: block;
}

.examples-container {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.example-pill {
  padding: 0.4rem 0.8rem;
  background: rgba(102, 126, 234, 0.05);
  border: 1px solid rgba(102, 126, 234, 0.15);
  border-radius: 20px;
  color: #667eea;
  font-size: 0.85rem;
  cursor: pointer;
  transition: all 0.3s ease;
}

.example-pill:hover {
  background: rgba(102, 126, 234, 0.1);
  transform: translateY(-1px);
  border-color: rgba(102, 126, 234, 0.25);
}

/* Search Types Grid */
.search-types-section .section-title {
  color: #1e293b;
  font-weight: 600;
  margin-bottom: 1rem;
}

.search-types-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1rem;
}

.search-type-card {
  padding: 1rem;
  background: #ffffff;
  border: 2px solid #f1f5f9;
  border-radius: 15px;
  cursor: pointer;
  transition: all 0.3s ease;
  display: block;
}

.search-type-card:hover {
  background: #f8fafc;
  transform: translateY(-2px);
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.08);
  border-color: #e2e8f0;
}

.search-type-card.active {
  background: #ffffff;
  border-color: #667eea;
  box-shadow: 0 0 20px rgba(102, 126, 234, 0.15);
}

.card-content {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.card-icon {
  width: 50px;
  height: 50px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.5rem;
  /* color: white; */
}

.all-types {
  background: linear-gradient(135deg, #667eea, #764ba2);
}

.cas-type {
  background: linear-gradient(135deg, #22c55e, #16a34a);
}

.ec-type {
  background: linear-gradient(135deg, #06b6d4, #0891b2);
}

.chemical-type {
  background: linear-gradient(135deg, #f59e0b, #d97706);
}

.substance-type {
  background: linear-gradient(135deg, #6b7280, #4b5563);
}

.card-info {
  flex: 1;
}

.card-title {
  color: #1e293b;
  font-weight: 600;
  font-size: 1rem;
  margin-bottom: 0.25rem;
}

.card-desc {
  color: #64748b;
  font-size: 0.85rem;
}

/* Advanced Options */
.advanced-options {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin: 2rem 0;
}

.options-group {
  display: flex;
  gap: 2rem;
}

.search-stats {
  display: flex;
  gap: 1rem;
  flex-wrap: wrap;
}

.stat-item {
  color: #64748b;
  font-size: 0.85rem;
  display: flex;
  align-items: center;
  gap: 0.3rem;
}

/* Action Buttons */
.action-buttons {
  display: flex;
  gap: 1rem;
  justify-content: center;
  margin-top: 2rem;
}

.btn-primary-modern {
  padding: 1rem 2.5rem;
  background: linear-gradient(135deg, #667eea, #764ba2);
  border: none;
  border-radius: 50px;
  color: white;
  font-size: 1.1rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  box-shadow: 0 10px 30px rgba(102, 126, 234, 0.3);
}

.btn-primary-modern:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 15px 40px rgba(102, 126, 234, 0.4);
}

.btn-primary-modern:disabled {
  opacity: 0.6;
  cursor: not-allowed;
  transform: none;
}

.btn-secondary-modern {
  padding: 1rem 2rem;
  background: white;
  border: 2px solid #e2e8f0;
  border-radius: 50px;
  color: #374151;
  font-size: 1rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.btn-secondary-modern:hover {
  background: #f8fafc;
  transform: translateY(-1px);
  border-color: #cbd5e1;
}

/* Loading Spinner */
.spinner {
  width: 20px;
  height: 20px;
  border: 2px solid #e2e8f0;
  border-radius: 50%;
  border-top-color: #667eea;
  animation: spin 1s ease-in-out infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

/* Enhanced Tab Styling - Light Version */
.search-mode-tabs {
  margin-bottom: 30px;
}

.tab-container {
  display: flex;
  background: #f8fafc;
  border-radius: 16px;
  padding: 6px;
  position: relative;
  box-shadow: inset 0 2px 8px rgba(0, 0, 0, 0.06);
  border: 1px solid rgba(0, 0, 0, 0.08);
}

.tab-btn {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 14px 24px;
  border: none;
  background: transparent;
  border-radius: 12px;
  cursor: pointer;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  font-weight: 500;
  font-size: 15px;
  color: #64748b;
  position: relative;
  z-index: 2;
  gap: 8px;
  min-height: 52px;
}

.tab-btn i {
  font-size: 16px;
  transition: all 0.3s ease;
}

.tab-btn:hover {
  color: #475569;
  background: rgba(255, 255, 255, 0.5);
  transform: translateY(-1px);
}

.tab-btn.active {
  color: white;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  box-shadow:
    0 4px 20px rgba(102, 126, 234, 0.3),
    0 2px 8px rgba(0, 0, 0, 0.1);
  transform: translateY(-2px);
}

.tab-btn.active i {
  transform: scale(1.1);
}

/* Responsive Design */
@media (max-width: 768px) {
  .hero-title {
    font-size: 2rem;
    /* ลดเพิ่มเติมใน mobile */
  }

  .search-panel {
    padding: 1.5rem !important;
  }

  .search-types-grid {
    grid-template-columns: 1fr;
  }

  .advanced-options {
    flex-direction: column;
    gap: 1rem;
    align-items: flex-start;
  }

  .action-buttons {
    flex-direction: column;
    align-items: center;
  }

  .btn-primary-modern,
  .btn-secondary-modern {
    width: 100%;
    justify-content: center;
  }

  .tab-btn {
    font-size: 14px;
    padding: 12px 16px;
    min-height: 48px;
  }

  .tab-btn i {
    font-size: 14px;
  }
}

/* Additional Light Theme Enhancements */
body {
  background: #f8fafc;
  color: #1e293b;
}

/* Scrollbar Styling for Light Theme */
::-webkit-scrollbar {
  width: 8px;
}

::-webkit-scrollbar-track {
  background: #f1f5f9;
}

::-webkit-scrollbar-thumb {
  background: #cbd5e1;
  border-radius: 4px;
}

::-webkit-scrollbar-thumb:hover {
  background: #94a3b8;
}

/* Focus outlines for accessibility */
*:focus {
  outline: 2px solid #667eea;
  outline-offset: 2px;
}

/* Selection styling */
::selection {
  background: rgba(102, 126, 234, 0.2);
  color: #1e293b;
}

/* Premium Batch Search Styles */

/* Premium Instruction Card */
.premium-instruction-card {
  position: relative;
  background: linear-gradient(135deg, rgba(255, 215, 0, 0.1), rgba(255, 107, 107, 0.1));
  border: 1px solid rgba(255, 215, 0, 0.3);
  border-radius: 20px;
  overflow: hidden;
}

.instruction-glow {
  position: absolute;
  top: -2px;
  left: -2px;
  right: -2px;
  bottom: -2px;
  background: linear-gradient(45deg, #ffd700, #ff6b6b, #667eea, #764ba2);
  border-radius: 22px;
  z-index: -1;
  animation: glow-rotate 3s linear infinite;
  opacity: 0.7;
}

@keyframes glow-rotate {
  0% {
    transform: rotate(0deg);
  }

  100% {
    transform: rotate(360deg);
  }
}

.instruction-content {
  display: flex;
  align-items: center;
  gap: 1.5rem;
  padding: 2rem;
  background: rgba(255, 255, 255, 0.05);
  backdrop-filter: blur(10px);
  position: relative;
}

.instruction-icon {
  width: 60px;
  height: 60px;
  background: linear-gradient(135deg, #ffd700, #ff6b6b);
  border-radius: 15px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.8rem;
  color: white;
  box-shadow: 0 10px 30px rgba(255, 215, 0, 0.3);
}

.instruction-text {
  flex: 1;
}

.instruction-text h6 {
  color: white;
  font-weight: 700;
  font-size: 1.1rem;
  margin-bottom: 0.5rem;
}

.instruction-text p {
  color: rgba(255, 255, 255, 0.8);
  margin: 0;
  line-height: 1.4;
  font-size: 0.95rem;
}

.instruction-animation {
  display: flex;
  gap: 0.5rem;
  align-items: center;
}

.pulse-dot {
  width: 12px;
  height: 12px;
  background: #ffd700;
  border-radius: 50%;
  animation: pulse-wave 2s ease-in-out infinite;
}

.pulse-dot.delay-1 {
  animation-delay: 0.3s;
}

.pulse-dot.delay-2 {
  animation-delay: 0.6s;
}

@keyframes pulse-wave {

  0%,
  60%,
  100% {
    transform: scale(0.8);
    opacity: 0.5;
  }

  30% {
    transform: scale(1.2);
    opacity: 1;
  }
}

/* Advanced Input Section */
.batch-input-advanced {
  position: relative;
}

.input-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
}

.input-title {
  color: white;
  font-weight: 600;
  font-size: 1rem;
}

.input-status {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.status-indicator {
  width: 10px;
  height: 10px;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.3);
  transition: all 0.3s ease;
}

.status-indicator.active {
  background: #28a745;
  box-shadow: 0 0 10px rgba(40, 167, 69, 0.5);
  animation: pulse-indicator 2s ease-in-out infinite;
}

@keyframes pulse-indicator {

  0%,
  100% {
    opacity: 1;
  }

  50% {
    opacity: 0.5;
  }
}

.status-text {
  color: rgba(255, 255, 255, 0.7);
  font-size: 0.85rem;
}

.textarea-container {
  position: relative;
}

.premium-textarea {
  width: 100%;
  padding: 2rem;
  font-size: 1rem;
  border: 2px solid rgba(255, 255, 255, 0.2);
  border-radius: 20px;
  background: rgba(255, 255, 255, 0.05);
  color: white;
  transition: all 0.4s ease;
  font-family: 'SF Mono', 'Monaco', 'Cascadia Code', monospace;
  resize: vertical;
  min-height: 200px;
  backdrop-filter: blur(10px);
}

.premium-textarea::placeholder {
  color: rgba(255, 255, 255, 0.4);
  line-height: 1.6;
}

.premium-textarea:focus {
  outline: none;
  border-color: #ffd700;
  background: rgba(255, 255, 255, 0.08);
  box-shadow: 0 0 30px rgba(255, 215, 0, 0.2),
    inset 0 0 20px rgba(255, 255, 255, 0.05);
}

.live-count-badge {
  position: absolute;
  top: 15px;
  right: 15px;
  background: rgba(40, 167, 69, 0.9);
  color: white;
  padding: 0.5rem 1rem;
  border-radius: 25px;
  font-size: 0.8rem;
  font-weight: 600;
  backdrop-filter: blur(10px);
  animation: bounce-in 0.5s ease-out;
}

@keyframes bounce-in {
  0% {
    transform: scale(0) rotate(45deg);
    opacity: 0;
  }

  50% {
    transform: scale(1.1) rotate(0deg);
  }

  100% {
    transform: scale(1) rotate(0deg);
    opacity: 1;
  }
}

/* Enhanced Action Buttons */
.batch-actions {
  display: flex;
  gap: 1rem;
  margin-top: 1rem;
  justify-content: center;
}

.action-btn {
  position: relative;
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 50px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  overflow: hidden;
  font-size: 0.9rem;
  backdrop-filter: blur(10px);
}

.action-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.action-btn.danger {
  background: rgba(255, 107, 107, 0.2);
  color: #ff6b6b;
  border: 1px solid rgba(255, 107, 107, 0.3);
}

.action-btn.danger:hover:not(:disabled) {
  background: rgba(255, 107, 107, 0.3);
  transform: translateY(-2px);
  box-shadow: 0 10px 25px rgba(255, 107, 107, 0.3);
}

.action-btn.success {
  background: rgba(40, 167, 69, 0.2);
  color: #28a745;
  border: 1px solid rgba(40, 167, 69, 0.3);
}

.action-btn.success:hover:not(:disabled) {
  background: rgba(40, 167, 69, 0.3);
  transform: translateY(-2px);
  box-shadow: 0 10px 25px rgba(40, 167, 69, 0.3);
}

.action-btn.primary {
  background: rgba(255, 215, 0, 0.2);
  color: #ffd700;
  border: 1px solid rgba(255, 215, 0, 0.3);
}

.action-btn.primary:hover:not(:disabled) {
  background: rgba(255, 215, 0, 0.3);
  transform: translateY(-2px);
  box-shadow: 0 10px 25px rgba(255, 215, 0, 0.3);
}

.btn-shine {
  position: absolute;
  top: -50%;
  left: -50%;
  width: 200%;
  height: 200%;
  background: linear-gradient(45deg, transparent, rgba(255, 255, 255, 0.1), transparent);
  transform: rotate(45deg);
  transition: all 0.6s;
  opacity: 0;
}

.action-btn:hover .btn-shine {
  animation: shine-effect 0.6s ease-in-out;
}

@keyframes shine-effect {
  0% {
    transform: translateX(-100%) rotate(45deg);
    opacity: 0;
  }

  50% {
    opacity: 1;
  }

  100% {
    transform: translateX(100%) rotate(45deg);
    opacity: 0;
  }
}

/* Interactive Stats Dashboard */
.batch-processing-display {
  background: rgba(255, 255, 255, 0.03);
  border-radius: 20px;
  padding: 2rem;
  border: 1px solid rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(20px);
}

.stats-dashboard {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(160px, 1fr));
  gap: 1rem;
  margin-bottom: 2rem;
}

.stat-card {
  position: relative;
  background: rgba(255, 255, 255, 0.5);
  border-radius: 15px;
  padding: 1.5rem;
  border: 1px solid rgba(255, 255, 255, 0.1);
  transition: all 0.3s ease;
  overflow: hidden;
}

.stat-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 15px 40px rgba(0, 0, 0, 0.2);
}

.stat-card.valid-items {
  border-color: rgba(40, 167, 69, 0.3);
  background: rgba(40, 167, 69, 0.1);
}

.stat-card.cas-items {
  border-color: rgba(23, 162, 184, 0.3);
  background: rgba(23, 162, 184, 0.1);
}

.stat-card.ec-items {
  border-color: rgba(111, 66, 193, 0.3);
  background: rgba(111, 66, 193, 0.1);
}

.stat-card.invalid-items {
  border-color: rgba(255, 193, 7, 0.3);
  background: rgba(255, 193, 7, 0.1);
}

.stat-icon {
  font-size: 1.5rem;
  margin-bottom: 1rem;
  opacity: 0.8;
}

.valid-items .stat-icon {
  color: #28a745;
}

.cas-items .stat-icon {
  color: #17a2b8;
}

.ec-items .stat-icon {
  color: #6f42c1;
}

.invalid-items .stat-icon {
  color: #ffc107;
}

.stat-number {
  font-size: 2rem;
  font-weight: 800;
  color: white;
  margin-bottom: 0.5rem;
  animation: count-up 1s ease-out;
}

.stat-label {
  /* color: rgba(255, 255, 255, 0.7); */
  font-size: 0.85rem;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

@keyframes count-up {
  from {
    opacity: 0;
    transform: translateY(20px);
  }

  to {
    opacity: 1;
    transform: translateY(0);
  }
}

/* Interactive Preview */
.interactive-preview {
  border-top: 1px solid rgba(255, 255, 255, 0.1);
  padding-top: 2rem;
}

.preview-controls {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

.preview-title-section {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.preview-title {
  color: white;
  font-weight: 700;
  margin: 0;
  font-size: 1.1rem;
}

.preview-count {
  background: rgba(255, 215, 0, 0.2);
  color: #ffd700;
  padding: 0.25rem 0.75rem;
  border-radius: 15px;
  font-size: 0.8rem;
  font-weight: 600;
}

.toggle-view-btn {
  background: rgba(255, 255, 255, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.2);
  color: rgba(255, 255, 255, 0.8);
  padding: 0.5rem 1rem;
  border-radius: 25px;
  font-size: 0.85rem;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
}

.toggle-view-btn:hover {
  background: rgba(255, 255, 255, 0.2);
  color: white;
  transform: translateY(-1px);
}

.preview-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1rem;
}

.preview-chip {
  position: relative;
  background: rgba(255, 255, 255, 0.05);
  border-radius: 12px;
  padding: 1rem;
  border: 1px solid rgba(255, 255, 255, 0.1);
  transition: all 0.3s ease;
  overflow: hidden;
  cursor: pointer;
}

.preview-chip:hover {
  transform: translateY(-3px);
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.2);
}

.preview-chip.cas_no {
  border-left: 4px solid #28a745;
}

.preview-chip.ec_no {
  border-left: 4px solid #17a2b8;
}

.preview-chip.name {
  border-left: 4px solid #6f42c1;
}

.preview-chip.invalid {
  border-left: 4px solid #ffc107;
}

.chip-type-indicator {
  position: absolute;
  top: 8px;
  right: 8px;
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: currentColor;
}

.chip-value {
  display: block;
  color: white;
  font-weight: 600;
  font-size: 0.9rem;
  margin-bottom: 0.5rem;
  font-family: 'SF Mono', monospace;
}

.chip-type-label {
  color: rgba(255, 255, 255, 0.6);
  font-size: 0.7rem;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  margin-bottom: 0.5rem;
}

.chip-remove {
  position: absolute;
  top: 8px;
  right: 24px;
  background: rgba(255, 107, 107, 0.2);
  border: 1px solid rgba(255, 107, 107, 0.3);
  color: #ff6b6b;
  width: 24px;
  height: 24px;
  border-radius: 50%;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.8rem;
  transition: all 0.3s ease;
  opacity: 0;
}

.preview-chip:hover .chip-remove {
  opacity: 1;
}

.chip-remove:hover {
  background: rgba(255, 107, 107, 0.4);
  transform: scale(1.1);
}

.chip-hover-effect {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: linear-gradient(45deg, transparent, rgba(255, 255, 255, 0.05), transparent);
  transform: translateX(-100%);
  transition: all 0.6s ease;
}

.preview-chip:hover .chip-hover-effect {
  transform: translateX(100%);
}

/* Readiness Indicator */
.readiness-indicator {
  margin-top: 2rem;
  background: linear-gradient(135deg, rgba(40, 167, 69, 0.1), rgba(32, 201, 151, 0.1));
  border: 1px solid rgba(40, 167, 69, 0.3);
  border-radius: 15px;
  padding: 1.5rem;
  position: relative;
  overflow: hidden;
}

.readiness-content {
  display: flex;
  align-items: center;
  gap: 1rem;
  position: relative;
  z-index: 2;
}

.readiness-icon {
  width: 50px;
  height: 50px;
  background: rgba(40, 167, 69, 0.2);
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.5rem;
  color: #28a745;
}

.readiness-text {
  flex: 1;
}

.readiness-title {
  display: block;
  color: white;
  font-weight: 700;
  font-size: 1.1rem;
  margin-bottom: 0.25rem;
}

.readiness-subtitle {
  color: rgba(255, 255, 255, 0.7);
  font-size: 0.9rem;
}

.readiness-pulse {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: linear-gradient(90deg, transparent, rgba(40, 167, 69, 0.1), transparent);
  animation: pulse-sweep 3s ease-in-out infinite;
}

@keyframes pulse-sweep {

  0%,
  100% {
    transform: translateX(-100%);
  }

  50% {
    transform: translateX(100%);
  }
}

/* Responsive Design for Batch Search */
@media (max-width: 768px) {
  .instruction-content {
    flex-direction: column;
    text-align: center;
    gap: 1rem;
  }

  .instruction-animation {
    justify-content: center;
  }

  .batch-actions {
    flex-direction: column;
    align-items: center;
  }

  .action-btn {
    width: 100%;
    justify-content: center;
    max-width: 250px;
  }

  .stats-dashboard {
    grid-template-columns: repeat(2, 1fr);
  }

  .preview-grid {
    grid-template-columns: 1fr;
  }

  .preview-controls {
    flex-direction: column;
    gap: 1rem;
    align-items: flex-start;
  }

  .readiness-content {
    flex-direction: column;
    text-align: center;
    gap: 1rem;
  }
}

@media (max-width: 480px) {
  .premium-instruction-card {
    margin: 0 -1rem;
  }

  .instruction-content {
    padding: 1.5rem;
  }

  .premium-textarea {
    padding: 1.5rem;
    min-height: 150px;
  }

  .stats-dashboard {
    grid-template-columns: 1fr;
  }

  .stat-card {
    padding: 1rem;
  }

  .preview-title-section {
    flex-direction: column;
    align-items: flex-start;
    gap: 0.5rem;
  }
}

/* Results Summary */
.results-summary {
  padding: 2rem;
}

.summary-content {
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 2rem;
}

.summary-stats {
  display: flex;
  gap: 2rem;
}

.stat-card {
  text-align: center;
}

.stat-number {
  font-size: 2rem;
  font-weight: 800;
  margin-bottom: 0.5rem;
}

.stat-label {
  font-size: 0.9rem;
  opacity: 0.8;
}

.svhc-stat .stat-number {
  color: #ff6b6b;
}

.regular-stat .stat-number {
  color: #28a745;
}

.total-stat .stat-number {
  color: #ffd700;
}

.search-info {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  /* color: rgba(255, 255, 255, 0.8); */
}

.search-query {
  font-weight: 600;
  /* color: white; */
}

/* Results Cards */
.results-card {
  margin-bottom: 2rem;
}

.results-header {
  padding: 1.5rem 2rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
}

.svhc-header {
  background: linear-gradient(135deg, #ff6b6b, #ee5a5a);
}

.regular-header {
  background: linear-gradient(135deg, #28a745, #20c997);
}

.header-content {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.header-icon {
  width: 60px;
  height: 60px;
  background: rgba(255, 255, 255, 0.2);
  border-radius: 15px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.8rem;
  color: white;
}

.header-title {
  color: white;
  font-weight: 700;
  margin: 0;
}

.header-subtitle {
  color: rgba(255, 255, 255, 0.8);
  margin: 0;
  font-size: 0.9rem;
}

.header-badge {
  padding: 0.5rem 1rem;
  border-radius: 20px;
  font-weight: 600;
  font-size: 0.85rem;
}

.danger-badge {
  background: rgba(255, 255, 255, 0.2);
  color: white;
}

.success-badge {
  background: rgba(255, 255, 255, 0.2);
  color: white;
}

/* Modern Table */
.table-container {
  overflow-x: auto;
}

.modern-table {
  width: 100%;
  border-collapse: collapse;
  background: rgba(255, 255, 255, 0.05);
}

.modern-table thead th {
  padding: 1rem;
  background: rgba(255, 255, 255, 0.1);
  color: white;
  font-weight: 600;
  font-size: 0.9rem;
  border-bottom: 2px solid rgba(255, 255, 255, 0.1);
}

.modern-table tbody tr {
  border-bottom: 1px solid rgba(255, 255, 255, 0.05);
  transition: all 0.3s ease;
}

.modern-table tbody tr:hover {
  background: rgba(255, 255, 255, 0.05);
}

.modern-table td {
  padding: 1rem;
  color: rgba(255, 255, 255, 0.9);
  max-width: 200px;
}

.cas-number,
.ec-number {
  font-family: 'Courier New', monospace;
  background: rgba(255, 255, 255, 0.1);
  padding: 0.25rem 0.5rem;
  border-radius: 6px;
  font-size: 0.85rem;
}

.chemical-name {
  font-weight: 600;
  color: white;
}

.reason-text,
.uses-text,
.scope-text,
.identifier-text {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.status-badge,
.threshold-badge {
  padding: 0.25rem 0.75rem;
  border-radius: 12px;
  font-size: 0.8rem;
  font-weight: 600;
}

.status-danger {
  background: #ff6b6b;
  color: white;
}

.status-info {
  background: #17a2b8;
  color: white;
}

.threshold-badge {
  background: #ffc107;
  color: #212529;
}

.action-btn {
  width: 35px;
  height: 35px;
  background: rgba(255, 255, 255, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 8px;
  color: white;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  justify-content: center;
}

.action-btn:hover {
  background: rgba(255, 255, 255, 0.2);
  transform: scale(1.1);
}

/* Pagination */
.pagination-card {
  padding: 1.5rem 2rem;
  background-color: rgba(255, 255, 255, 0.5);
  ;
}

.pagination-content {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.pagination-info {
  color: rgba(255, 255, 255, 0.8);
  font-size: 0.9rem;
}

.pagination-controls {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.pagination-btn {
  padding: 0.75rem 1.5rem;
  background: rgba(255, 255, 255, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 25px;
  color: white;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.pagination-btn:hover:not(:disabled) {
  background: rgba(255, 255, 255, 0.2);
  transform: translateY(-1px);
}

.pagination-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.current-page {
  padding: 0.75rem 1rem;
  background: #ffd700;
  color: #212529;
  border-radius: 50%;
  font-weight: 700;
  min-width: 40px;
  text-align: center;
}

/* No Results */
.no-results-card {
  padding: 3rem 2rem;
}

.no-results-icon {
  font-size: 4rem;
  color: rgba(255, 255, 255, 0.5);
  margin-bottom: 2rem;
}

.no-results-title {
  /* color: white; */
  font-weight: 700;
  margin-bottom: 1rem;
}

.no-results-text {
  /* color: rgba(255, 255, 255, 0.7); */
  margin-bottom: 2rem;
  line-height: 1.6;
}

.no-results-suggestions {
  text-align: left;
  max-width: 400px;
  margin: 0 auto 2rem;
}

.no-results-suggestions h6 {
  /* color: white; */
  font-weight: 600;
  margin-bottom: 1rem;
}

.no-results-suggestions ul {
  /* color: rgba(255, 255, 255, 0.7); */
  padding-left: 1.5rem;
}

.no-results-suggestions li {
  margin-bottom: 0.5rem;
  line-height: 1.4;
}

/* Detail Modal */
.detail-modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0, 0, 0, 0.8);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 9999;
  padding: 2rem;
}

.detail-modal {
  max-width: 600px;
  width: 100%;
  max-height: 80vh;
  overflow-y: auto;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.5rem 2rem;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
}

.modal-header h5 {
  color: white;
  font-weight: 700;
  margin: 0;
}

.modal-close {
  background: none;
  border: none;
  color: rgba(255, 255, 255, 0.6);
  font-size: 1.2rem;
  cursor: pointer;
  padding: 0.5rem;
  border-radius: 50%;
  transition: all 0.3s ease;
}

.modal-close:hover {
  background: rgba(255, 255, 255, 0.1);
  color: white;
}

.modal-content {
  padding: 2rem;
}

.detail-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1.5rem;
}

.detail-item {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.detail-item.full-width {
  grid-column: 1 / -1;
}

.detail-item label {
  color: rgba(255, 255, 255, 0.7);
  font-size: 0.9rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.detail-item span {
  color: white;
  font-size: 1rem;
  line-height: 1.5;
  word-break: break-word;
}

.status-danger {
  color: #ff6b6b !important;
  font-weight: 600;
}

.status-info {
  color: #17a2b8 !important;
  font-weight: 600;
}

/* Responsive Design */
@media (max-width: 768px) {
  .hero-title {
    font-size: 2.5rem;
  }

  .search-panel {
    padding: 1.5rem !important;
  }

  .search-types-grid {
    grid-template-columns: 1fr;
  }

  .advanced-options {
    flex-direction: column;
    gap: 1rem;
    align-items: flex-start;
  }

  .summary-content {
    flex-direction: column;
    text-align: center;
  }

  .summary-stats {
    justify-content: center;
  }

  .results-header {
    flex-direction: column;
    gap: 1rem;
    text-align: center;
  }

  .pagination-content {
    flex-direction: column;
    gap: 1rem;
    text-align: center;
  }

  .detail-grid {
    grid-template-columns: 1fr;
  }

  .action-buttons {
    flex-direction: column;
    align-items: center;
  }

  .btn-primary-modern,
  .btn-secondary-modern {
    width: 100%;
    justify-content: center;
  }
}

.batch-instructions-card {
  border-left: 4px solid #3b82f6;
  background: rgba(59, 130, 246, 0.05);
}

.instructions-header {
  display: flex;
  align-items: flex-start;
  gap: 1rem;
  padding: 1.5rem;
}

.instructions-icon {
  width: 40px;
  height: 40px;
  background: #3b82f6;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.instructions-icon i {
  color: white;
  font-size: 1.2rem;
}

.instructions-content {
  flex: 1;
}

.instructions-title {
  color: #1e293b;
  font-weight: 600;
  margin: 0 0 0.5rem 0;
  font-size: 1rem;
}

.instructions-text {
  color: #64748b;
  margin: 0;
  line-height: 1.5;
  font-size: 0.9rem;
}

/* Input Section */
.batch-input-section {
  background: white;
  border-radius: 16px;
  padding: 1.5rem;
  border: 2px solid #e2e8f0;
}

.input-label-section {
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 1rem;
}

.input-main-label {
  color: #1e293b;
  font-weight: 600;
  font-size: 1.1rem;
  display: flex;
  align-items: center;
}

.format-examples {
  display: flex;
  gap: 0.5rem;
  flex-wrap: wrap;
}

.format-example {
  background: #f1f5f9;
  color: #475569;
  padding: 0.25rem 0.5rem;
  border-radius: 6px;
  font-size: 0.75rem;
  font-family: monospace;
}

.textarea-wrapper {
  position: relative;
}

.batch-textarea {
  width: 100%;
  min-height: 180px;
  padding: 1.5rem;
  border: 2px solid #e2e8f0;
  border-radius: 12px;
  background: #fafafa;
  color: #1e293b;
  font-family: 'SF Mono', Consolas, monospace;
  font-size: 0.9rem;
  line-height: 1.5;
  resize: vertical;
  transition: all 0.3s ease;
}

.batch-textarea::placeholder {
  color: #94a3b8;
  line-height: 1.6;
}

.batch-textarea:focus {
  outline: none;
  border-color: #3b82f6;
  background: white;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.processing-indicator {
  position: absolute;
  top: 12px;
  right: 12px;
  background: rgba(34, 197, 94, 0.9);
  color: white;
  padding: 0.5rem 0.75rem;
  border-radius: 20px;
  font-size: 0.8rem;
  font-weight: 500;
  backdrop-filter: blur(10px);
}

.indicator-content {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.pulse-dot {
  width: 8px;
  height: 8px;
  background: white;
  border-radius: 50%;
  animation: pulse 1.5s ease-in-out infinite;
}

@keyframes pulse {

  0%,
  100% {
    opacity: 1;
  }

  50% {
    opacity: 0.5;
  }
}

/* Action Buttons */
.input-actions {
  display: flex;
  gap: 0.75rem;
}

.action-btn {
  padding: 0.5rem 1rem;
  border: none;
  border-radius: 8px;
  font-weight: 500;
  font-size: 0.85rem;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
}

.action-btn.sample {
  background: #3b82f6;
  color: white;
  width: 50%;
}

.action-btn.sample:hover {
  background: #2563eb;
  transform: translateY(-1px);
  width: 50%;
}

.action-btn.clear {
  background: #f3f4f6;
  color: #6b7280;
  border: 1px solid #d1d5db;
  width: 50%;
}

.action-btn.clear:hover {
  background: #e5e7eb;
  color: #374151;
  width: 50%;
}

/* Preview Section */
.batch-preview-section {
  background: white;
  border-radius: 16px;
  padding: 1.5rem;
  border: 2px solid #e2e8f0;
}

.preview-summary {
  border-bottom: 1px solid #f1f5f9;
  padding-bottom: 1rem;
}

.summary-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
}

.summary-title {
  color: #1e293b;
  font-weight: 600;
  margin: 0;
  display: flex;
  align-items: center;
}

.total-count {
  background: #3b82f6;
  color: white;
  padding: 0.25rem 0.75rem;
  border-radius: 12px;
  font-size: 0.8rem;
  font-weight: 600;
}

.type-breakdown {
  display: flex;
  gap: 1.5rem;
  flex-wrap: wrap;
}

.type-stat {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.type-stat .count {
  font-weight: 700;
  font-size: 1.1rem;
}

.type-stat .label {
  color: #64748b;
  font-size: 0.85rem;
}

.type-stat.cas .count {
  color: #059669;
}

.type-stat.ec .count {
  color: #0284c7;
}

.type-stat.names .count {
  color: #7c3aed;
}

.type-stat.invalid .count {
  color: #ea580c;
}

/* Items Grid */
.items-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 0.75rem;
  margin-top: 1rem;
}

.batch-item {
  border: 2px solid #f1f5f9;
  border-radius: 12px;
  transition: all 0.3s ease;
  background: white;
}

.batch-item:hover {
  border-color: #e2e8f0;
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.batch-item.cas_no {
  border-left-color: #059669;
}

.batch-item.ec_no {
  border-left-color: #0284c7;
}

.batch-item.name {
  border-left-color: #7c3aed;
}

.batch-item.invalid {
  border-left-color: #ea580c;
  background: #fef7f5;
}

.item-content {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 1rem;
}

.item-type-badge {
  width: 36px;
  height: 36px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.item-type-badge.cas_no {
  background: rgba(5, 150, 105, 0.1);
  color: #059669;
}

.item-type-badge.ec_no {
  background: rgba(2, 132, 199, 0.1);
  color: #0284c7;
}

.item-type-badge.name {
  background: rgba(124, 58, 237, 0.1);
  color: #7c3aed;
}

.item-type-badge.invalid {
  background: rgba(234, 88, 12, 0.1);
  color: #ea580c;
}

.item-details {
  flex: 1;
  min-width: 0;
}

.item-value {
  color: #1e293b;
  font-weight: 600;
  font-size: 0.9rem;
  margin-bottom: 0.25rem;
  word-break: break-word;
}

.item-type-label {
  color: #64748b;
  font-size: 0.75rem;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.remove-item {
  width: 24px;
  height: 24px;
  background: #f1f5f9;
  border: none;
  border-radius: 50%;
  color: #64748b;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.remove-item:hover {
  background: #fee2e2;
  color: #dc2626;
  transform: scale(1.1);
}

/* Show More/Less */
.show-more-item,
.show-less-item {
  grid-column: 1 / -1;
  display: flex;
  justify-content: center;
}

.show-more-btn,
.show-less-btn {
  padding: 0.75rem 1.5rem;
  background: #f8fafc;
  border: 2px dashed #cbd5e1;
  border-radius: 12px;
  color: #64748b;
  cursor: pointer;
  transition: all 0.3s ease;
  font-weight: 500;
}

.show-more-btn:hover,
.show-less-btn:hover {
  background: #f1f5f9;
  border-color: #94a3b8;
  color: #475569;
}

/* Warnings and Indicators */
.invalid-warning {
  background: rgba(251, 146, 60, 0.1);
  border: 1px solid rgba(251, 146, 60, 0.2);
  border-radius: 12px;
  padding: 1rem;
  margin-top: 1rem;
}

.warning-content {
  color: #ea580c;
  font-size: 0.9rem;
  display: flex;
  align-items: flex-start;
}

.ready-indicator {
  background: linear-gradient(135deg, rgba(34, 197, 94, 0.1), rgba(16, 185, 129, 0.1));
  border: 1px solid rgba(34, 197, 94, 0.2);
  border-radius: 12px;
  padding: 1.5rem;
  margin-top: 1rem;
}

.ready-content {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.ready-icon {
  width: 50px;
  height: 50px;
  background: rgba(34, 197, 94, 0.2);
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #16a34a;
  font-size: 1.5rem;
}

.ready-text {
  flex: 1;
}

.ready-title {
  color: #16a34a;
  font-weight: 700;
  font-size: 1.1rem;
  margin-bottom: 0.25rem;
}

.ready-subtitle {
  color: #64748b;
  font-size: 0.9rem;
}

/* Empty State */
.batch-empty-state {
  text-align: center;
  padding: 3rem 2rem;
}

.empty-content {
  max-width: 400px;
  margin: 0 auto;
}

.empty-icon {
  width: 80px;
  height: 80px;
  background: #f1f5f9;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 1.5rem;
  font-size: 2rem;
  color: #94a3b8;
}

.empty-title {
  color: #1e293b;
  font-weight: 600;
  margin-bottom: 1rem;
}

.empty-text {
  color: #64748b;
  line-height: 1.6;
  margin: 0;
}

/* Responsive Design */
@media (max-width: 768px) {
  .input-label-section {
    flex-direction: column;
    align-items: flex-start;
    gap: 0.75rem;
  }

  .type-breakdown {
    justify-content: center;
  }

  .items-grid {
    grid-template-columns: 1fr;
  }

  .summary-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 0.75rem;
  }

  .ready-content {
    flex-direction: column;
    text-align: center;
  }
}

.improved-table {
  background: rgba(255, 255, 255, 0.95) !important;
  border-collapse: separate;
  border-spacing: 0;
  border-radius: 10px;
  overflow: hidden;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.1);
}

.improved-table thead {
  background: linear-gradient(135deg, #2c3e50, #34495e) !important;
}

.improved-table thead th {
  color: white !important;
  font-weight: 600 !important;
  padding: 1rem !important;
  border: none !important;
  font-size: 0.9rem;
  text-align: left;
}

.improved-row {
  border-bottom: 1px solid rgba(0, 0, 0, 0.05) !important;
  transition: all 0.3s ease;
  background: rgba(255, 255, 255, 0.8) !important;
}

.improved-row:hover {
  background: rgba(52, 152, 219, 0.15) !important;
  transform: translateY(-1px);
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.improved-row td {
  padding: 1rem !important;
  border: none !important;
  vertical-align: middle;
  color: #2c3e50 !important;
  font-size: 0.9rem;
}

/* สไตล์เฉพาะสำหรับข้อมูลต่างๆ */
.improved-cas,
.improved-ec {
  font-family: 'Courier New', monospace !important;
  font-weight: 600 !important;
  color: #e74c3c !important;
  background: rgba(231, 76, 60, 0.1) !important;
  padding: 4px 8px !important;
  border-radius: 4px !important;
  display: inline-block;
  font-size: 0.85rem;
}

.improved-chemical {
  font-weight: 600 !important;
  color: #2c3e50 !important;
  font-size: 0.95rem;
}

.improved-identifier {
  color: #7f8c8d !important;
  font-size: 0.85rem !important;
  font-style: italic;
}

.improved-text {
  color: #34495e !important;
  max-width: 200px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  line-height: 1.4;
}

.improved-badge {
  padding: 6px 12px !important;
  border-radius: 15px !important;
  font-size: 0.8rem !important;
  font-weight: 600 !important;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.improved-badge.status-danger {
  background: linear-gradient(135deg, #ff6b6b, #ee5a52) !important;
  color: white !important;
}

.improved-badge.status-info {
  background: linear-gradient(135deg, #3498db, #2980b9) !important;
  color: white !important;
}

.improved-threshold {
  background: linear-gradient(135deg, #f39c12, #e67e22) !important;
  color: white !important;
  padding: 6px 12px !important;
  border-radius: 12px !important;
  font-size: 0.8rem !important;
  font-weight: 600 !important;
  display: inline-block;
}

.improved-muted {
  color: #95a5a6 !important;
  font-style: italic;
}

/* Responsive สำหรับหน้าจอเล็ก */
@media (max-width: 768px) {
  .improved-table {
    font-size: 0.8rem !important;
  }

  .improved-table thead th,
  .improved-row td {
    padding: 0.75rem 0.5rem !important;
  }

  .improved-text {
    max-width: 150px;
  }

  .improved-cas,
  .improved-ec {
    font-size: 0.75rem !important;
    padding: 2px 6px !important;
  }
}

@media (max-width: 576px) {
  .improved-text {
    max-width: 100px;
  }

  .improved-table thead th,
  .improved-row td {
    padding: 0.5rem 0.25rem !important;
    font-size: 0.75rem !important;
  }
}

.not-found-section {
  padding: 1rem;
  background: rgba(255, 255, 255, 0.5) !important;
}

/* Detect Box */
.result-box {
  display: flex;
  align-items: center;
  justify-content: center;
  background: transparent;
  margin-top: 20px;
}

.result-button {
  padding: 20px 60px;
  border: none;
  border-radius: 25px;
  font-size: 28px;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: 0 8px 25px rgba(0, 0, 0, 0.15);
  position: relative;
  overflow: hidden;
  min-width: 200px;
}

.result-button:before {
  content: '';
  position: absolute;
  top: 0;
  left: -100%;
  width: 100%;
  height: 100%;
  background: linear-gradient(90deg, transparent, rgba(255, 255, 255, 0.2), transparent);
  transition: left 0.5s;
}

.result-button:hover:before {
  left: 100%;
}

.result-button:hover {
  transform: translateY(-3px);
  box-shadow: 0 12px 35px rgba(0, 0, 0, 0.25);
}

.result-button:active {
  transform: translateY(-1px);
  transition: all 0.1s ease;
}

.detect-button {
  background: linear-gradient(135deg, #ff6b6b, #ee5a52);
  color: white;
  text-shadow: 0 2px 4px rgba(0, 0, 0, 0.3);
  width: 60%;
}

.detect-button:hover {
  background: linear-gradient(135deg, #ff5252, #e53e3e);
  width: 60%;
}

.not-detect-button {
  background: linear-gradient(135deg, #51cf66, #40c057);
  color: white;
  text-shadow: 0 2px 4px rgba(0, 0, 0, 0.3);
  width: 60%;
}

.not-detect-button:hover {
  background: linear-gradient(135deg, #40c057, #37b24d);
  width: 60%;
}

@media (max-width: 768px) {
  .result-box {
    min-width: auto;
  }
}
</style>