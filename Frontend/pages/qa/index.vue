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
/* === Page === */
.bg-gradient-modern {
  background: var(--bg);
  min-height: 100vh;
  position: relative;
}

/* Remove floating shapes */
.floating-shapes, .shape, .shape-1, .shape-2, .shape-3, .shape-4 { display: none; }

/* === Hero === */
.glass-effect {
  background: var(--surface);
  border-radius: var(--r-xl) !important;
  border: 1px solid var(--border) !important;
  box-shadow: var(--shadow-md) !important;
  position: relative;
  overflow: hidden;
}
.glass-effect::before {
  content: ''; position: absolute; top: 0; left: 0; right: 0; height: 4px;
  background: linear-gradient(90deg, var(--dept-qa) 0%, var(--brand) 100%);
}

.hero-card { padding: var(--s-8) !important; }

.hero-title {
  font-size: var(--fs-3xl) !important;
  font-weight: 800 !important;
  color: var(--text-1) !important;
  letter-spacing: -0.03em;
  line-height: 1.15;
}

.gradient-text { color: var(--brand); font-weight: 700; }

.hero-subtitle { font-size: var(--fs-base) !important; color: var(--text-2) !important; }

.feature-badge {
  padding: 5px var(--s-3);
  background: var(--brand-light);
  border-radius: var(--r-full);
  color: var(--brand);
  font-size: var(--fs-xs);
  font-weight: 600;
  display: inline-flex; align-items: center; gap: var(--s-1);
  border: 1px solid rgba(92,107,192,0.2);
}

.btn-new-search {
  padding: var(--s-4) var(--s-10);
  background: linear-gradient(135deg, var(--dept-qa) 0%, var(--brand) 100%);
  border-radius: var(--r-full);
  color: #fff;
  font-size: var(--fs-md);
  font-weight: 700;
  border: none; cursor: pointer;
  box-shadow: 0 4px 20px rgba(0,131,143,0.35);
  transition: all var(--t-mid) var(--ease);
  font-family: var(--font);
  display: inline-flex; align-items: center; gap: var(--s-2);
}
.btn-new-search:hover { transform: translateY(-2px); box-shadow: 0 8px 28px rgba(0,131,143,0.4); }

/* === Search Panel === */
.search-panel {
  padding: var(--s-8) !important;
  background: var(--surface) !important;
  border-radius: var(--r-xl);
  border: 1px solid var(--border);
  box-shadow: var(--shadow-md);
}

.search-input-container { position: relative; }

.search-input-wrapper { position: relative; }

.search-input {
  padding: var(--s-4) var(--s-8) var(--s-4) 52px;
  font-size: var(--fs-md) !important;
  border: 2px solid var(--border) !important;
  border-radius: var(--r-full) !important;
  background: var(--surface) !important;
  color: var(--text-1) !important;
  font-family: var(--font) !important;
  width: 100%;
  transition: border-color var(--t-fast) var(--ease), box-shadow var(--t-fast) var(--ease);
  outline: none;
}
.search-input:focus {
  border-color: var(--dept-qa) !important;
  box-shadow: 0 0 0 4px rgba(0,131,143,0.12) !important;
}

/* === Search Type Cards === */
.search-type-card {
  padding: var(--s-4) var(--s-5);
  border-radius: var(--r-lg);
  border: 2px solid var(--border);
  background: var(--surface);
  cursor: pointer;
  transition: all var(--t-fast) var(--ease);
  display: flex; align-items: center; gap: var(--s-3);
}
.search-type-card:hover { border-color: var(--dept-qa); background: var(--brand-xlight); transform: translateY(-1px); }
.search-type-card.active { border-color: var(--dept-qa); background: var(--brand-xlight); }
.search-type-card.selected { border-color: var(--dept-qa); background: var(--brand-xlight); box-shadow: 0 0 0 3px rgba(0,131,143,0.15); }

.type-icon {
  width: 40px; height: 40px;
  border-radius: var(--r-md);
  display: flex; align-items: center; justify-content: center;
  font-size: 18px; flex-shrink: 0; font-weight: 700;
}

.type-name { font-size: var(--fs-sm); font-weight: 700; color: var(--text-1); }
.type-example { font-size: var(--fs-xs); color: var(--text-3); font-family: var(--font-mono); }

/* === Batch Search === */
.batch-input-section, .batch-progress, .batch-search-section, .batch-preview-section { margin-bottom: var(--s-4); }

.batch-textarea {
  font-family: var(--font-mono) !important;
  font-size: var(--fs-sm) !important;
  border: 1.5px solid var(--border) !important;
  border-radius: var(--r-md) !important;
  background: var(--surface) !important;
  color: var(--text-1) !important;
  padding: var(--s-3) !important;
  resize: vertical;
}

.batch-item {
  padding: var(--s-3) var(--s-4);
  background: var(--surface-2);
  border-radius: var(--r-md);
  border: 1px solid var(--border-light);
  margin-bottom: var(--s-2);
  font-size: var(--fs-sm);
  display: flex; align-items: center; gap: var(--s-2);
}

/* === Result Section === */
.result-button {
  padding: var(--s-3) var(--s-5);
  border-radius: var(--r-full);
  border: 2px solid var(--border);
  background: var(--surface);
  cursor: pointer;
  font-size: var(--fs-sm); font-weight: 600;
  font-family: var(--font);
  transition: all var(--t-fast) var(--ease);
  color: var(--text-2);
}
.result-button:hover, .result-button.detect-button { border-color: var(--dept-qa); color: var(--dept-qa); background: var(--brand-xlight); }
.result-button.not-detect-button { border-color: var(--border); color: var(--text-3); }

/* === Auto Detection === */
.auto-detection {
  display: flex; align-items: center; gap: var(--s-2);
  padding: var(--s-2) var(--s-3);
  background: var(--success-bg);
  border-radius: var(--r-md);
  border: 1px solid var(--success-border);
  font-size: var(--fs-xs); color: var(--success); font-weight: 600;
}

/* === Batch Summary === */
.batch-summary {
  background: var(--surface); border-radius: var(--r-lg); border: 1px solid var(--border);
  padding: var(--s-5); box-shadow: var(--shadow-sm);
}
.batch-summary-title { font-size: var(--fs-md); font-weight: 700; color: var(--text-1); margin-bottom: var(--s-4); }

.batch-empty-state { text-align: center; padding: var(--s-10) 0; color: var(--text-3); }

/* === Search Button === */
.search-all-btn, button[class*="search"] {
  background: linear-gradient(135deg, var(--dept-qa) 0%, var(--brand) 100%) !important;
  border-radius: var(--r-full) !important;
  font-weight: 700 !important;
  box-shadow: 0 4px 16px rgba(0,131,143,0.35) !important;
  transition: all var(--t-mid) var(--ease) !important;
  border: none !important;
}

/* === Advanced Options === */
.advanced-options { padding: var(--s-4); background: var(--surface-2); border-radius: var(--r-md); border: 1px solid var(--border-light); }

/* === Result Cards === */
.result-card {
  background: var(--surface); border-radius: var(--r-lg); border: 1px solid var(--border);
  box-shadow: var(--shadow-sm); overflow: hidden;
  transition: box-shadow var(--t-mid) var(--ease);
  animation: fadeInUp 0.3s var(--ease) both;
}
.result-card:hover { box-shadow: var(--shadow-md); }

.result-header {
  padding: var(--s-4) var(--s-5);
  background: var(--surface-2);
  border-bottom: 1px solid var(--border);
  display: flex; align-items: center; gap: var(--s-3);
}

.svhc-badge, .regular-badge {
  display: inline-flex; align-items: center; gap: 4px;
  padding: 3px var(--s-2); border-radius: var(--r-full);
  font-size: var(--fs-xs); font-weight: 700; letter-spacing: 0.04em;
}
.svhc-badge    { background: var(--error-bg);   color: var(--error);   border: 1px solid var(--error-border); }
.regular-badge { background: var(--success-bg); color: var(--success); border: 1px solid var(--success-border); }

.result-name { font-size: var(--fs-md); font-weight: 700; color: var(--text-1); margin-bottom: var(--s-1); }
.result-cas, .result-ec { font-size: var(--fs-xs); color: var(--text-3); font-family: var(--font-mono); }

.detail-grid {
  display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: var(--s-3); padding: var(--s-5);
}
.detail-item { display: flex; flex-direction: column; gap: 2px; }
.detail-label { font-size: var(--fs-xs); font-weight: 700; color: var(--text-3); text-transform: uppercase; letter-spacing: 0.06em; }
.detail-value { font-size: var(--fs-sm); color: var(--text-1); font-weight: 500; }

/* === Batch Progress === */
.batch-progress {
  background: var(--surface); border-radius: var(--r-lg); border: 1px solid var(--border); padding: var(--s-5);
}
.progress-bar {
  height: 6px; background: var(--surface-3); border-radius: var(--r-full); overflow: hidden; margin: var(--s-2) 0;
}
.progress-fill { height: 100%; background: linear-gradient(90deg, var(--dept-qa), var(--brand)); border-radius: var(--r-full); transition: width var(--t-slow) var(--ease); }

/* === Animations === */
@keyframes fadeInUp {
  from { opacity: 0; transform: translateY(12px); }
  to   { opacity: 1; transform: translateY(0); }
}

@keyframes float {
  0%, 100% { transform: translateY(0) rotate(0deg); }
  50% { transform: translateY(-20px) rotate(180deg); }
}

/* === Responsive === */
@media (max-width: 768px) {
  .hero-title { font-size: var(--fs-2xl) !important; }
  .search-panel { padding: var(--s-5) !important; }
  .btn-new-search { padding: var(--s-3) var(--s-6); font-size: var(--fs-base); }
}
</style>