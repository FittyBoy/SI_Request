<!-- ====================================== -->
<!-- SCRIPT SECTION -->
<!-- ====================================== -->
<script setup lang="ts">
// ===== IMPORTS =====
import { ref, watch, onMounted, onBeforeUnmount, computed, nextTick } from "vue";
import ThicknessMonitorDialog from "@/components/ThicknessMonitorDialog.vue";

// ===== PROPS =====
const props = withDefaults(defineProps<{
    initialLot?: string | null
    initialSize?: string | null
    autoOpenMonitor?: boolean
}>(), {
    initialLot: null,
    initialSize: null,
    autoOpenMonitor: false,
})
import {
    Chart as ChartJS,
    ScatterController,
    CategoryScale,
    LinearScale,
    PointElement,
    Title,
    Tooltip,
    Legend,
    type ChartData,
} from "chart.js";
import { Scatter } from "vue-chartjs";
import annotationPlugin from 'chartjs-plugin-annotation';
import html2canvas from "html2canvas";
import jsPDF from "jspdf";
import { format } from 'date-fns';
import {
    BarChart3, Package, Settings, Calendar, FilterX,
    FileDown, Download, AlertCircle, RefreshCw, CheckCircle,
    PauseCircle, XCircle, AlertTriangle, HelpCircle,
    Clock, Power, PowerOff
} from 'lucide-vue-next';

// ===== TYPE DEFINITIONS =====
interface ChartDataset {
    Label: string;
    Data: { X: number; Y: number }[];
    BackgroundColor: string;
    PointRadius: number;
    PointHoverRadius: number;
    ShowLine: boolean;
}

interface LotChart {
    LotId: string;
    Datasets: ChartDataset[];
    Size: string;
    Status: string;
    PoLot: string;
    MinValue?: number;
    MaxValue?: number;
    AvgValue?: number;
    TotalPoints?: number;
    DateProcess?: string;
    LotPo?: string;
    // Raw record fields for Monitor dialog
    ThBefore?: number | null;
    ProcessTime?: number | null;
    Rate?: number | null;
    Margin?: number | null;
    Process?: string;
    McPo?: string;
    NoPo?: string;
    Program?: string | null;
    Result?: string;
    Ca1In?: (number | null)[];
    Ca1Out?: (number | null)[];
    Ca2In?: (number | null)[];
    Ca2Out?: (number | null)[];
    Ca3In?: (number | null)[];
    Ca3Out?: (number | null)[];
    Ca4In?: (number | null)[];
    Ca4Out?: (number | null)[];
    Ca5In?: (number | null)[];
    Ca5Out?: (number | null)[];
}

interface McPoItem {
    Label: string;
    Value: string;
}

interface ThresholdItem {
    Value: number;
    Label: string;
    Color: string;
}

interface SeriesMapItem {
    Key: string;
    Label: string;
    Color: string;
    Row: number;
}

// ===== CHART.JS SETUP =====
ChartJS.register(
    ScatterController,
    CategoryScale,
    LinearScale,
    PointElement,
    Title,
    Tooltip,
    Legend,
    annotationPlugin
);

// ===== STATE MANAGEMENT =====
const Lot = ref<string>("LOTNAME");
const ImobileSize = ref<string>("");
const ImobileSizes = ref<string[]>([]);
const ChartData = ref<ChartData<"scatter">>({ labels: [], datasets: [] });
const FilterMc = ref<string | null>(null);
const ChartWrappers = ref<HTMLElement[]>([]);

// Loading & Error States
const IsLoading = ref<boolean>(false);
const HasError = ref<boolean>(false);
const IsExporting = ref<boolean>(false);
const ShowExportDialog = ref<boolean>(false);
const ExportProgress = ref<number>(0);
const IsRefreshing = ref<boolean>(false);

// Auto-refresh States
const AutoRefreshEnabled = ref<boolean>(true);
const RefreshInterval = ref<number>(300); // seconds (5 minutes default)
const TimeUntilNextRefresh = ref<number>(300);
const RefreshTimer = ref<ReturnType<typeof setInterval> | null>(null);
const CountdownTimer = ref<ReturnType<typeof setInterval> | null>(null);
const LastRefreshTime = ref<Date | null>(null);
const RefreshIntervalOptions = [
    { title: '1 minute', value: 60 },
    { title: '3 minutes', value: 180 },
    { title: '5 minutes', value: 300 },
    { title: '10 minutes', value: 600 },
    { title: '15 minutes', value: 900 },
];

// UI States
const ViewMode = ref<'horizontal' | 'vertical'>('horizontal');
const Menu = ref<boolean>(false);
const SelectedDate = ref<string | null>(null);
const DisplayDate = ref<string>('');

// Monitor Dialog States
const ShowMonitorDialog = ref<boolean>(false);
const SelectedLotChart = ref<LotChart | null>(null);

// ===== CONSTANTS =====
const SeriesMap: SeriesMapItem[] = [
    { Key: "Ca1In", Label: "CA1 IN", Color: "black", Row: 10 },
    { Key: "Ca1Out", Label: "CA1 OUT", Color: "black", Row: 9 },
    { Key: "Ca2In", Label: "CA2 IN", Color: "black", Row: 8 },
    { Key: "Ca2Out", Label: "CA2 OUT", Color: "black", Row: 7 },
    { Key: "Ca3In", Label: "CA3 IN", Color: "black", Row: 6 },
    { Key: "Ca3Out", Label: "CA3 OUT", Color: "black", Row: 5 },
    { Key: "Ca4In", Label: "CA4 IN", Color: "black", Row: 4 },
    { Key: "Ca4Out", Label: "CA4 OUT", Color: "black", Row: 3 },
    { Key: "Ca5In", Label: "CA5 IN", Color: "black", Row: 2 },
    { Key: "Ca5Out", Label: "CA5 OUT", Color: "black", Row: 1 },
];

const ProductSizeThresholdMap: Record<string, ThresholdItem[]> = {
    "76x76x0.14": [
        { Value: 0.147, Label: "Hold", Color: "blue" },
        { Value: 0.142, Label: "Re-screen", Color: "orange" },
        { Value: 0.133, Label: "Target", Color: "green" },
        { Value: 0.138, Label: "Targetbar", Color: "green" },
        { Value: 0.130, Label: "Re-screen", Color: "orange" },
        { Value: 0.130, Label: "Scrap", Color: "red" },
    ],
    "76x76x0.14 Sample 10 Pcs.": [
        { Value: 0.147, Label: "Hold", Color: "blue" },
        { Value: 0.142, Label: "Re-screen", Color: "orange" },
        { Value: 0.133, Label: "Target", Color: "green" },
        { Value: 0.138, Label: "Targetbar", Color: "green" },
        { Value: 0.130, Label: "Re-screen", Color: "orange" },
        { Value: 0.130, Label: "Scrap", Color: "red" },
    ],
    "76x76x0.2 Sample 10 Pcs": [
        { Value: 0.209, Label: "Hold", Color: "blue" },
        { Value: 0.206, Label: "Re-screen", Color: "orange" },
        { Value: 0.195, Label: "Target", Color: "green" },
        { Value: 0.200, Label: "Targetbar", Color: "green" },
        { Value: 0.192, Label: "Re-screen", Color: "orange" },
        { Value: 0.192, Label: "Scrap", Color: "red" },
    ],
    "28x41.3x0.43": [
        { Value: 0.445, Label: "Hold", Color: "blue" },
        { Value: 0.416, Label: "Target", Color: "green" },
        { Value: 0.435, Label: "Targetbar", Color: "green" },
        { Value: 0.415, Label: "Scrap", Color: "red" },
    ],
    "29x30x0.6": [
        { Value: 0.615, Label: "Hold", Color: "blue" },
        { Value: 0.586, Label: "Target", Color: "green" },
        { Value: 0.610, Label: "Targetbar", Color: "green" },
        { Value: 0.585, Label: "Scrap", Color: "red" },
    ],
    "76x76x0.225  NFR": [
        { Value: 0.234, Label: "Hold", Color: "blue" },
        { Value: 0.231, Label: "Re-screen", Color: "orange" },
        { Value: 0.220, Label: "Target", Color: "green" },
        { Value: 0.225, Label: "Targetbar", Color: "green" },
        { Value: 0.217, Label: "Re-screen", Color: "orange" },
        { Value: 0.217, Label: "Scrap", Color: "red" },
    ],
    "AL50 76x76x0.225": [
        { Value: 0.234, Label: "Hold", Color: "blue" },
        { Value: 0.231, Label: "Re-screen", Color: "orange" },
        { Value: 0.220, Label: "Target", Color: "green" },
        { Value: 0.225, Label: "Targetbar", Color: "green" },
        { Value: 0.217, Label: "Re-screen", Color: "orange" },
        { Value: 0.217, Label: "Scrap", Color: "red" },
    ],
    "NFQ 76x76x0.25": [
        { Value: 0.260, Label: "Hold", Color: "blue" },
        { Value: 0.258, Label: "Re-screen", Color: "orange" },
        { Value: 0.250, Label: "Target", Color: "green" },
        { Value: 0.250, Label: "Targetbar", Color: "green" },
        { Value: 0.217, Label: "Re-screen", Color: "orange" },
        { Value: 0.217, Label: "Scrap", Color: "red" },
    ],
    "19.2x26.6x0.56": [
        { Value: 0.575, Label: "Hold", Color: "blue" },
        { Value: 0.570, Label: "Targetbar", Color: "green" },
        { Value: 0.546, Label: "Target", Color: "green" },
        { Value: 0.546, Label: "Scrap", Color: "red" },
    ],
    "19.2x26.6x0.61": [
        { Value: 0.625, Label: "Hold", Color: "blue" },
        { Value: 0.620, Label: "Targetbar", Color: "green" },
        { Value: 0.596, Label: "Target", Color: "green" },
        { Value: 0.596, Label: "Scrap", Color: "red" },
    ],
    "20.3x28.1x0.56": [
        { Value: 0.575, Label: "Hold", Color: "blue" },
        { Value: 0.565, Label: "Targetbar", Color: "green" },
        { Value: 0.546, Label: "Target", Color: "green" },
        { Value: 0.546, Label: "Scrap", Color: "red" },
    ],
    "21.5x29.5x0.32": [
        { Value: 0.335, Label: "Hold", Color: "blue" },
        { Value: 0.327, Label: "Targetbar", Color: "green" },
        { Value: 0.306, Label: "Target", Color: "green" },
        { Value: 0.306, Label: "Scrap", Color: "red" },
    ],
    "22x28x0.4": [
        { Value: 0.415, Label: "Hold", Color: "blue" },
        { Value: 0.410, Label: "Targetbar", Color: "green" },
        { Value: 0.386, Label: "Target", Color: "green" },
        { Value: 0.386, Label: "Scrap", Color: "red" },
    ],
    "46x54x0.5": [
        { Value: 0.510, Label: "Hold", Color: "blue" },
        { Value: 0.500, Label: "Targetbar", Color: "green" },
        { Value: 0.481, Label: "Target", Color: "green" },
        { Value: 0.481, Label: "Scrap", Color: "red" }
    ],
    "46x54x0.25": [
        { Value: 0.270, Label: "Hold", Color: "blue" },
        { Value: 0.260, Label: "Targetbar", Color: "green" },
        { Value: 0.231, Label: "Target", Color: "green" },
        { Value: 0.231, Label: "Scrap", Color: "red" }
    ],
    "30.2x43.9x0.7": [
        { Value: 0.720, Label: "Hold", Color: "blue" },
        { Value: 0.710, Label: "Targetbar", Color: "green" },
        { Value: 0.676, Label: "Target", Color: "green" },
        { Value: 0.676, Label: "Scrap", Color: "red" }
    ],
    "28x40x0.23": [
        { Value: 0.245, Label: "Hold", Color: "blue" },
        { Value: 0.235, Label: "Targetbar", Color: "green" },
        { Value: 0.216, Label: "Target", Color: "green" },
        { Value: 0.216, Label: "Scrap", Color: "red" }
    ],
    "28x40x0.43": [
        { Value: 0.445, Label: "Hold", Color: "blue" },
        { Value: 0.440, Label: "Targetbar", Color: "green" },
        { Value: 0.416, Label: "Target", Color: "green" },
        { Value: 0.416, Label: "Scrap", Color: "red" }
    ],
    "28.4x43.9x0.32": [
        { Value: 0.320, Label: "Hold", Color: "blue" },
        { Value: 0.315, Label: "Targetbar", Color: "green" },
        { Value: 0.306, Label: "Target", Color: "green" },
        { Value: 0.306, Label: "Scrap", Color: "red" }
    ],
    "29.2x40.8x0.23": [
        { Value: 0.245, Label: "Hold", Color: "blue" },
        { Value: 0.235, Label: "Targetbar", Color: "green" },
        { Value: 0.216, Label: "Target", Color: "green" },
        { Value: 0.216, Label: "Scrap", Color: "red" }
    ],
    "28.4x43.9x0.47": [
        { Value: 0.485, Label: "Hold", Color: "blue" },
        { Value: 0.475, Label: "Targetbar", Color: "green" },
        { Value: 0.456, Label: "Target", Color: "green" },
        { Value: 0.456, Label: "Scrap", Color: "red" }
    ],
    "28X41.3X0.7": [
        { Value: 0.715, Label: "Hold", Color: "blue" },
        { Value: 0.705, Label: "Targetbar", Color: "green" },
        { Value: 0.686, Label: "Target", Color: "green" },
        { Value: 0.686, Label: "Scrap", Color: "red" }
    ],
    "19.2x26.6x0.35": [
        { Value: 0.365, Label: "Hold", Color: "blue" },
        { Value: 0.355, Label: "Targetbar", Color: "green" },
        { Value: 0.336, Label: "Target", Color: "green" },
        { Value: 0.336, Label: "Scrap", Color: "red" }
    ],
    "76x76x0.2": [
        { Value: 0.209, Label: "Hold", Color: "blue" },
        { Value: 0.206, Label: "Re-screen", Color: "orange" },
        { Value: 0.195, Label: "Target", Color: "green" },
        { Value: 0.200, Label: "Targetbar", Color: "green" },
        { Value: 0.192, Label: "Re-screen", Color: "orange" },
        { Value: 0.192, Label: "Scrap", Color: "red" },
    ],
    //New Product
    "AL60 76x76x0.2": [
        { Value: 0.207, Label: "Hold", Color: "blue"},
        { Value: 0.205, Label: "Re-screen", Color: "orange" },
        { Value: 0.194, Label: "Target", Color: "green" },
        { Value: 0.200, Label: "Targetbar", Color: "green" },
        { Value: 0.192, Label: "Re-screen", Color: "orange" },
        { Value: 0.192, Label: "Scrap", Color: "red" }
    ]
};

// ===== API CALLS =====
const config = useRuntimeConfig();

const { data: Records, error: ErrorRecords, refresh: RefreshRecords } = await useFetch<any[]>("/api/SI25008", {
    baseURL: config.public.apiBase as string,
    method: "GET",
    headers: { "Content-Type": "application/json" },
    transform: (data: any) => Array.isArray(data) ? data : [],
    onResponseError({ response }) {
        console.error('❌ API Error:', response.status, response._data);
    }
});

const { data: ProductData, error: ErrorProduct, refresh: RefreshProduct } = await useFetch<string[]>("/api/SI25008/GetAllProduct", {
    baseURL: config.public.apiBase as string,
    method: "GET",
    headers: { "Content-Type": "application/json" },
    transform: (data: any) => Array.isArray(data) ? data : [],
});

// ===== UTILITY FUNCTIONS =====
const GetThresholdsForSize = (productSize?: string): ThresholdItem[] => {
    if (!productSize || typeof productSize !== 'string') return [];

    const cleanProductSize = productSize.trim().toLowerCase();
    const exactMatch = Object.keys(ProductSizeThresholdMap).find(key =>
        key.toLowerCase() === cleanProductSize
    );

    if (exactMatch) {
        return ProductSizeThresholdMap[exactMatch];
    }

    const partialMatch = Object.keys(ProductSizeThresholdMap).find(key =>
        cleanProductSize.includes(key.toLowerCase()) || key.toLowerCase().includes(cleanProductSize)
    );

    return partialMatch ? ProductSizeThresholdMap[partialMatch] : [];
};

const GetThresholdColor = (value: number, size: string): string => {
    const thresholds = GetThresholdsForSize(size);
    if (!thresholds || thresholds.length === 0) return '#E0E0E0';

    const sortedThresholds = [...thresholds].sort((a, b) => b.Value - a.Value);

    for (const threshold of sortedThresholds) {
        if (value >= threshold.Value) {
            return threshold.Color;
        }
    }

    return sortedThresholds[sortedThresholds.length - 1]?.Color || '#E0E0E0';
};

const GetStatusColor = (status: string): string => {
    if (!status || typeof status !== 'string') return 'grey darken-1';

    const trimmedStatus = status.trim().toLowerCase();
    const colorMap: Record<string, string> = {
        "hold": "#1976d2",
        "ok": "#388e3c",
        "scrap": "#d32f2f",
        "rescreen": "#f57c00",
        "scraprunbroken": "#c62828",
        "": "#616161",
    };

    return colorMap[trimmedStatus] || '#1976d2';
};

const GetStatusIcon = (status: string) => {
    const iconMap: Record<string, any> = {
        'OK': CheckCircle,
        'HOLD': PauseCircle,
        'Scrap': XCircle,
        'Rescreen': RefreshCw,
        'Scrap Run broken': AlertTriangle,
    };
    return iconMap[status] || HelpCircle;
};

const FormatDateToString = (date: string | Date): string => {
    return new Date(date).toISOString().split("T")[0];
};

const FormatDateToMMDD = (date: Date): string => {
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${day}${month}`;
};

const ExtractDateFromLotPo = (lotPo: string): string | null => {
    if (!lotPo || typeof lotPo !== 'string') return null;
    const dateStr = lotPo.substring(0, 4);
    if (!/^\d{4}$/.test(dateStr)) return null;
    return dateStr;
};

const GetSelectedDateMMDD = (selectedDate: string): string => {
    const date = new Date(selectedDate);
    return FormatDateToMMDD(date);
};

const ParseNoPo = (str: string) => {
    if (!str) return { Prefix: '', Num: Infinity, SuffixChar: '' };

    const parts = str.split('-');
    if (parts.length < 2) return { Prefix: str, Num: Infinity, SuffixChar: '' };

    const prefix = parts[0];
    let segment = parts[1];
    let suffixChar = '';

    if (segment.length > 1 && /[a-zA-Z]$/.test(segment)) {
        suffixChar = segment.slice(-1);
        segment = segment.slice(0, -1);
    }

    const num = parseInt(segment, 10);
    return {
        Prefix: prefix,
        Num: isNaN(num) ? Infinity : num,
        SuffixChar: suffixChar
    };
};

const CompareNoPo = (a: string, b: string): number => {
    const aParsed = ParseNoPo(a);
    const bParsed = ParseNoPo(b);

    if (aParsed.Prefix < bParsed.Prefix) return -1;
    if (aParsed.Prefix > bParsed.Prefix) return 1;
    if (aParsed.Num < bParsed.Num) return -1;
    if (aParsed.Num > bParsed.Num) return 1;
    if (aParsed.SuffixChar < bParsed.SuffixChar) return -1;
    if (aParsed.SuffixChar > bParsed.SuffixChar) return 1;

    return 0;
};

const SetChartWrapperRef = (el: any, index: number) => {
    if (el) {
        ChartWrappers.value[index] = el;
    }
};

const GetChartOptions = (size: string, minValue?: number, maxValue?: number) => {
    const sizeAnnotations = GetThresholdsForSize(size) ?? [];
    const annotations: any = {};

    if (sizeAnnotations.length > 0) {
        const sortedThresholds = [...sizeAnnotations].sort((a, b) => b.Value - a.Value);
        const allValues = sortedThresholds.map(a => a.Value);

        if (minValue !== undefined) allValues.push(minValue);
        if (maxValue !== undefined) allValues.push(maxValue);

        const yMin = allValues.length > 0 ? Math.min(...allValues) - 0.002 : 0;
        const yMax = allValues.length > 0 ? Math.max(...allValues) + 0.002 : 1;

        sortedThresholds.forEach((threshold, index) => {
            let zoneYMin: number;
            let zoneYMax: number;

            if (index === 0) {
                zoneYMin = threshold.Value;
                zoneYMax = yMax;
            } else if (index === sortedThresholds.length - 1) {
                zoneYMin = yMin;
                zoneYMax = threshold.Value;
            } else {
                zoneYMin = threshold.Value;
                zoneYMax = sortedThresholds[index - 1].Value;
            }

            const zoneColorMap: Record<string, string> = {
                'blue': 'rgba(33, 150, 243, 0.1)',
                'green': 'rgba(76, 175, 80, 0.1)',
                'orange': 'rgba(255, 152, 0, 0.1)',
                'red': 'rgba(244, 67, 54, 0.1)',
            };
            const zoneColor = zoneColorMap[threshold.Color.toLowerCase()] || 'rgba(158, 158, 158, 0.1)';

            annotations[`zone_${threshold.Label.toLowerCase()}_${index}`] = {
                type: 'box',
                xMin: -0.6,
                xMax: 9.6,
                yMin: zoneYMin,
                yMax: zoneYMax,
                backgroundColor: zoneColor,
                borderWidth: 0,
                z: -10,
            };

            const lineColorMap: Record<string, string> = {
                'blue': '#1976D2',
                'green': '#388E3C',
                'orange': '#F57C00',
                'red': '#D32F2F',
            };
            const lineColor = lineColorMap[threshold.Color.toLowerCase()] || '#616161';

            const isTarget = threshold.Label.toLowerCase() === 'targetbar';

            annotations[`line_${threshold.Label.toLowerCase()}_${index}`] = {
                type: 'line',
                yMin: threshold.Value,
                yMax: threshold.Value,
                borderColor: lineColor,
                borderWidth: 2,
                borderDash: isTarget ? [5, 3] : [],
            };
        });
    }

    const allThresholdValues = sizeAnnotations.map(a => a.Value);
    if (minValue !== undefined) allThresholdValues.push(minValue);
    if (maxValue !== undefined) allThresholdValues.push(maxValue);

    const yAxisMin = allThresholdValues.length > 0 ? Math.min(...allThresholdValues) - 0.002 : 0;
    const yAxisMax = allThresholdValues.length > 0 ? Math.max(...allThresholdValues) + 0.002 : 1;

    const orderedSeriesMap = [...SeriesMap].sort((a, b) => b.Row - a.Row);
    const xAxisLabels = orderedSeriesMap.map(series => series.Label);

    return {
        responsive: true,
        maintainAspectRatio: false,
        interaction: {
            intersect: true,
            mode: 'point' as const,
        },
        plugins: {
            legend: { display: false },
            annotation: {
                annotations,
                clip: false
            },
            tooltip: {
                backgroundColor: 'rgba(0, 0, 0, 0.8)',
                titleColor: 'white',
                bodyColor: 'white',
                borderColor: 'rgba(255, 255, 255, 0.2)',
                borderWidth: 1,
                cornerRadius: 8,
                displayColors: false,
                callbacks: {
                    title: function (context: any) {
                        if (context && context.length > 0) {
                            const xValue = Math.round(context[0].parsed.x);
                            return xAxisLabels[xValue] || '';
                        }
                        return '';
                    },
                    label: function (context: any) {
                        const value = context.parsed.y.toFixed(3);
                        const thresholds = GetThresholdsForSize(size);
                        let status = '';

                        if (thresholds && thresholds.length > 0) {
                            const sortedThresholds = [...thresholds].sort((a, b) => b.Value - a.Value);
                            for (const threshold of sortedThresholds) {
                                if (context.parsed.y >= threshold.Value) {
                                    status = ` (${threshold.Label})`;
                                    break;
                                }
                            }
                        }

                        return `Value: ${value}${status}`;
                    },
                }
            },
        },
        scales: {
            x: {
                type: "linear" as const,
                min: -0.5,
                max: orderedSeriesMap.length - 0.5,
                title: { display: false },
                ticks: {
                    stepSize: 1,
                    maxRotation: 45,
                    minRotation: 45,
                    font: { size: 10 },
                    color: '#616161',
                    callback: function (value: any) {
                        const intValue = Math.round(Number(value));
                        return xAxisLabels[intValue] || '';
                    }
                },
                grid: {
                    display: true,
                    color: 'rgba(0, 0, 0, 0.1)',
                    lineWidth: 1,
                }
            },
            y: {
                min: yAxisMin,
                max: yAxisMax,
                title: { display: false },
                ticks: {
                    stepSize: 0.001,
                    precision: 3,
                    font: { size: 10 },
                    color: '#616161',
                    callback: function (value: any) {
                        return Number(value).toFixed(3);
                    }
                },
                grid: {
                    display: true,
                    color: 'rgba(0, 0, 0, 0.1)',
                    lineWidth: 1,
                }
            },
        },
    };
};

// ===== COMPUTED PROPERTIES =====
const FilteredSizes = computed(() => {
    if (!Records.value || !ImobileSizes.value) return [];
    const sizesWithRecords = new Set(
        Records.value
            .map(r => r.ImobileSize)
            .filter(size => size && size.trim() !== "" && size !== "Product Size")
    );

    return ImobileSizes.value.filter(size => sizesWithRecords.has(size));
});

const McPoList = computed((): McPoItem[] => {
    if (!Records.value || Records.value.length === 0 || !ImobileSize.value) return [];

    const filtered = Records.value.filter(r => r.ImobileSize === ImobileSize.value);
    const uniqueMcPo = [...new Set(filtered.map(r => r.McPo))];

    return uniqueMcPo
        .map(mc => ({ Label: mc || 'Unknown', Value: mc || '' }))
        .sort((a, b) => {
            const numA = parseFloat(a.Value) || 0;
            const numB = parseFloat(b.Value) || 0;
            return numA - numB;
        });
});

const LotChartList = computed(() => {
    if (!Records.value || Records.value.length === 0 || !ImobileSize.value) return [];

    let sameSizeRecords = Records.value.filter(r => r.ImobileSize === ImobileSize.value);
    if (FilterMc.value) {
        sameSizeRecords = sameSizeRecords.filter(r => r.McPo === FilterMc.value);
    }

    if (sameSizeRecords.length === 0) return [];

    let filteredRecords: any[];

    if (SelectedDate.value) {
        const targetMMDD = GetSelectedDateMMDD(SelectedDate.value);
        filteredRecords = sameSizeRecords.filter(rec => {
            const lotPoDate = ExtractDateFromLotPo(rec.LotPo);
            return lotPoDate === targetMMDD;
        });
    } else {
        const todayMMDD = FormatDateToMMDD(new Date());
        const todayRecords = sameSizeRecords.filter(rec => {
            const lotPoDate = ExtractDateFromLotPo(rec.LotPo);
            return lotPoDate === todayMMDD;
        });

        if (todayRecords.length > 0) {
            filteredRecords = todayRecords;
        } else {
            const latestRecord = sameSizeRecords.reduce((latest, current) =>
                new Date(current.DateProcess) > new Date(latest.DateProcess) ? current : latest
            );
            const latestDate = new Date(latestRecord.DateProcess).toISOString().split("T")[0];

            filteredRecords = sameSizeRecords.filter(rec => {
                const recordDate = new Date(rec.DateProcess).toISOString().split("T")[0];
                return recordDate === latestDate;
            });
        }
    }

    if (filteredRecords.length === 0) return [];

    const sortedRecords = filteredRecords.sort((a, b) => CompareNoPo(a.NoPo, b.NoPo));

    return sortedRecords.map(rec => {
        const allValues: number[] = [];

        SeriesMap.forEach(({ Key }) => {
            const values = (rec as any)[Key];
            if (!Array.isArray(values)) return;

            values
                .filter((v): v is number => typeof v === 'number')
                .forEach((value) => {
                    allValues.push(value);
                });
        });

        const datasets: ChartDataset[] = SeriesMap
            .map(({ Key, Label, Color }, index) => {
                const values = (rec as any)[Key];
                if (!Array.isArray(values)) return null;

                const dataPoints = values
                    .filter((v): v is number => typeof v === 'number')
                    .map((v) => ({
                        X: index,
                        Y: v,
                        Value: v
                    }));

                return {
                    Label,
                    Data: dataPoints,
                    BackgroundColor: Color,
                    PointRadius: 5,
                    PointHoverRadius: 7,
                    ShowLine: false,
                };
            })
            .filter((dataset): dataset is ChartDataset => dataset !== null);

        const minValue = allValues.length > 0 ? Math.min(...allValues) : 0;
        const maxValue = allValues.length > 0 ? Math.max(...allValues) : 0;
        const totalPoints = allValues.length;

        const avgValue = allValues.length > 0
            ? allValues.reduce((sum, val) => sum + val, 0) / allValues.length
            : 0;

        return {
            LotId: rec.ImobileLot || rec.LotId || 'Unknown',
            Datasets: datasets,
            Size: rec.ImobileSize || 'Unknown',
            Status: rec.Status || '',
            PoLot: `${rec.LotPo}-${rec.McPo}-${rec.NoPo}`,
            MinValue: Number(minValue.toFixed(4)),
            MaxValue: Number(maxValue.toFixed(4)),
            AvgValue: Number(avgValue.toFixed(4)),
            TotalPoints: totalPoints,
            DateProcess: rec.DateProcess,
            LotPo: rec.LotPo,
            // Raw record fields for Monitor dialog
            ThBefore: rec.ThBefore ?? rec.thBefore,
            ProcessTime: rec.ProcessTime ?? rec.processTime,
            Rate: rec.PoRate ?? rec.poRate,
            Margin: rec.Margin ?? rec.margin,
            Process: rec.Process ?? rec.process,
            McPo: rec.McPo ?? rec.mcPo,
            NoPo: rec.NoPo ?? rec.noPo,
            Program: rec.Program ?? rec.program,
            Result: rec.Result ?? rec.result,
            Ca1In: rec.Ca1In ?? rec.ca1In ?? [],
            Ca1Out: rec.Ca1Out ?? rec.ca1Out ?? [],
            Ca2In: rec.Ca2In ?? rec.ca2In ?? [],
            Ca2Out: rec.Ca2Out ?? rec.ca2Out ?? [],
            Ca3In: rec.Ca3In ?? rec.ca3In ?? [],
            Ca3Out: rec.Ca3Out ?? rec.ca3Out ?? [],
            Ca4In: rec.Ca4In ?? rec.ca4In ?? [],
            Ca4Out: rec.Ca4Out ?? rec.ca4Out ?? [],
            Ca5In: rec.Ca5In ?? rec.ca5In ?? [],
            Ca5Out: rec.Ca5Out ?? rec.ca5Out ?? [],
        };
    });
});

const FormatValue = (value: number, decimalPlaces: number = 3): string => {
    if (typeof value !== 'number' || isNaN(value)) return 'N/A';

    const multiplier = Math.pow(10, decimalPlaces);
    const roundedValue = Math.round(value * multiplier) / multiplier;

    return roundedValue.toFixed(decimalPlaces);
};

const FormatTimeRemaining = (seconds: number): string => {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins}:${secs.toString().padStart(2, '0')}`;
};

// ===== ACTION FUNCTIONS =====
const ClearAllFilters = () => {
    ImobileSize.value = '';
    FilterMc.value = null;
    ClearDateFilter();
};

const RetryLoadData = async () => {
    IsLoading.value = true;
    HasError.value = false;

    try {
        await Promise.all([RefreshRecords(), RefreshProduct()]);
        HasError.value = false;
        LastRefreshTime.value = new Date();
        console.log('✅ Data refreshed successfully');
    } catch (error) {
        console.error('❌ Error retrying data load:', error);
        HasError.value = true;
    } finally {
        IsLoading.value = false;
    }
};

// Silent refresh without showing loading state
const SilentRefreshData = async () => {
    if (IsRefreshing.value) return; // Prevent multiple simultaneous refreshes

    IsRefreshing.value = true;
    HasError.value = false;

    try {
        // Fetch new data silently
        await Promise.all([RefreshRecords(), RefreshProduct()]);
        HasError.value = false;
        LastRefreshTime.value = new Date();
        console.log('✅ Data auto-refreshed successfully');
    } catch (error) {
        console.error('❌ Error during auto-refresh:', error);
        HasError.value = true;
    } finally {
        IsRefreshing.value = false;
    }
};

const OnDateSelected = (val: string) => {
    if (val) {
        DisplayDate.value = format(new Date(val), 'dd/MM/yyyy');
        SelectedDate.value = val;
    }
    Menu.value = false;
};

const ClearDateFilter = () => {
    SelectedDate.value = null;
    DisplayDate.value = '';
};

// ===== AUTO-REFRESH FUNCTIONS =====
const StartAutoRefresh = () => {
    StopAutoRefresh();

    if (!AutoRefreshEnabled.value) return;

    TimeUntilNextRefresh.value = RefreshInterval.value;

    // Countdown timer (updates every second)
    CountdownTimer.value = setInterval(() => {
        if (TimeUntilNextRefresh.value > 0) {
            TimeUntilNextRefresh.value--;
        }
    }, 1000);

    // Refresh timer (triggers silent data reload)
    RefreshTimer.value = setInterval(async () => {
        // Only refresh if tab is visible and auto-refresh is enabled
        if (!document.hidden && AutoRefreshEnabled.value) {
            console.log('🔄 Auto-refreshing data silently...');
            await SilentRefreshData(); // Use silent refresh instead
            TimeUntilNextRefresh.value = RefreshInterval.value;
        }
    }, RefreshInterval.value * 1000);

    console.log(`✅ Auto-refresh started: Every ${RefreshInterval.value} seconds`);
};

const StopAutoRefresh = () => {
    if (RefreshTimer.value) {
        clearInterval(RefreshTimer.value);
        RefreshTimer.value = null;
    }
    if (CountdownTimer.value) {
        clearInterval(CountdownTimer.value);
        CountdownTimer.value = null;
    }
    console.log('⏸️ Auto-refresh stopped');
};

const ToggleAutoRefresh = () => {
    AutoRefreshEnabled.value = !AutoRefreshEnabled.value;
    if (AutoRefreshEnabled.value) {
        StartAutoRefresh();
    } else {
        StopAutoRefresh();
    }
};

const ManualRefresh = async () => {
    console.log('🔄 Manual refresh triggered');
    await SilentRefreshData(); // Use silent refresh for manual too
    if (AutoRefreshEnabled.value) {
        StartAutoRefresh(); // Restart timer
    }
};

const OnRefreshIntervalChange = () => {
    if (AutoRefreshEnabled.value) {
        StartAutoRefresh(); // Restart with new interval
    }
};

// ===== MONITOR DIALOG =====
const OpenMonitorDialog = (lotChart: LotChart) => {
    SelectedLotChart.value = lotChart;
    ShowMonitorDialog.value = true;
};

// ===== PDF EXPORT FUNCTIONS =====
const DownloadPDF = async () => {
    if (!ChartWrappers.value || ChartWrappers.value.length === 0) {
        console.error("No chart wrappers found.");
        return;
    }

    IsExporting.value = true;
    ShowExportDialog.value = true;
    ExportProgress.value = 0;

    try {
        await nextTick();

        const pdf = new jsPDF({
            orientation: "landscape",
            unit: "mm",
            format: "a4",
        });

        const chartsPerPage = 5;
        const gap = 0.25;
        const pdfWidth = pdf.internal.pageSize.getWidth();
        const pdfHeight = pdf.internal.pageSize.getHeight();

        const chartImages: string[] = [];

        for (let i = 0; i < ChartWrappers.value.length; i++) {
            const wrapper = ChartWrappers.value[i];
            if (!document.body.contains(wrapper)) continue;

            try {
                const canvas = await html2canvas(wrapper, {
                    backgroundColor: "#ffffff",
                    scale: 2,
                    willReadFrequently: true,
                });

                const imgData = canvas.toDataURL("image/png");
                chartImages.push(imgData);

                ExportProgress.value = ((i + 1) / ChartWrappers.value.length) * 80;
            } catch (err) {
                console.error("Error capturing chart:", err);
            }
        }

        for (let page = 0; page * chartsPerPage < chartImages.length; page++) {
            if (page > 0) pdf.addPage();

            for (let i = 0; i < chartsPerPage; i++) {
                const index = page * chartsPerPage + i;
                if (index >= chartImages.length) break;

                const img = chartImages[index];
                const imgWidth = (pdfWidth - gap * (chartsPerPage - 1)) / chartsPerPage;
                const imgHeight = 150;
                const x = i * (imgWidth + gap);
                const y = (pdfHeight - imgHeight) / 2;

                pdf.addImage(img, "PNG", x, y, imgWidth, imgHeight);
            }
        }

        ExportProgress.value = 100;

        const filename = `Charts-${ImobileSize.value}-${new Date().toISOString().slice(0, 10)}.pdf`;
        pdf.save(filename);

    } catch (error) {
        console.error('Error generating PDF:', error);
    } finally {
        setTimeout(() => {
            IsExporting.value = false;
            ShowExportDialog.value = false;
            ExportProgress.value = 0;
        }, 1000);
    }
};

const ExportSingleChart = async (index: number) => {
    const wrapper = ChartWrappers.value[index];
    if (!wrapper) return;

    try {
        const canvas = await html2canvas(wrapper, {
            backgroundColor: "#ffffff",
            scale: 2,
            willReadFrequently: true,
        });

        const imgData = canvas.toDataURL("image/png");
        const link = document.createElement('a');
        link.download = `Chart-${LotChartList.value[index].LotId}-${new Date().toISOString().slice(0, 10)}.png`;
        link.href = imgData;
        link.click();
    } catch (error) {
        console.error('Error exporting single chart:', error);
    }
};

// ===== WATCHERS =====
watch(ProductData, (newVal) => {
    if (Array.isArray(newVal)) {
        const cleaned = newVal.filter(
            (size) => size && size.trim() !== "" && size !== "Product Size"
        );
        ImobileSizes.value = cleaned;

        if (!cleaned.includes(ImobileSize.value)) {
            ImobileSize.value = cleaned.length > 0 ? cleaned[0] : "";
        }
    }
});

watch(() => ErrorRecords.value, (error) => {
    if (error) {
        console.error("Error loading main data:", error);
    }
});

watch(() => ErrorProduct.value, (error) => {
    if (error) {
        console.error("Error loading product list:", error);
    }
});

// ===== LIFECYCLE =====
onMounted(async () => {
    IsLoading.value = true;
    try {
        await Promise.all([RefreshRecords(), RefreshProduct()]);

        if (ProductData.value && Array.isArray(ProductData.value)) {
            const cleaned = ProductData.value.filter(
                (size) => size && size.trim() !== "" && size !== "Product Size"
            );
            ImobileSizes.value = cleaned;

            // ── Deep-link: apply props first ──────────────────────────────
            if (props.initialSize && cleaned.includes(props.initialSize)) {
                // Size ที่ส่งมาจาก query param ตรงกับข้อมูล → ใช้เลย
                ImobileSize.value = props.initialSize;
            } else if (props.initialLot && Records.value) {
                // หา size จาก lot ที่ระบุมา
                const matchRec = Records.value.find(
                    r => (r.ImobileLot || r.LotId) === props.initialLot
                );
                if (matchRec) ImobileSize.value = matchRec.ImobileSize;
            } else {
                // fallback เดิม
                const validRecord = Records.value?.find(r =>
                    ImobileSizes.value.includes(r.ImobileSize)
                );
                if (validRecord) {
                    Lot.value = validRecord.LotId || 'LOTNAME';
                    ImobileSize.value = validRecord.ImobileSize;
                } else if (Records.value?.[0]) {
                    Lot.value = Records.value[0].LotId || 'LOTNAME';
                    ImobileSize.value = Records.value[0].ImobileSize;
                }
            }
        }

        HasError.value = false;
        LastRefreshTime.value = new Date();

        // Start auto-refresh
        if (AutoRefreshEnabled.value) {
            StartAutoRefresh();
        }

        // ── Deep-link: auto-open Monitor dialog ───────────────────────────
        if (props.autoOpenMonitor && props.initialLot) {
            await nextTick();
            // รอให้ LotChartList computed มีข้อมูล
            await nextTick();
            const target = LotChartList.value.find(
                lc => lc.LotId === props.initialLot
            ) ?? LotChartList.value[0] ?? null;
            if (target) OpenMonitorDialog(target);
        }

    } catch (error) {
        console.error('❌ Error during component initialization:', error);
        HasError.value = true;
    } finally {
        IsLoading.value = false;
    }
});

onBeforeUnmount(() => {
    StopAutoRefresh();
});
</script>
<!-- ====================================== -->
<!-- TEMPLATE SECTION -->
<!-- ====================================== -->
<template>
    <div class="thickness-chart-container">
        <!-- ===== LOADING STATE ===== -->
        <div v-if="IsLoading" class="loading-container">
            <v-progress-circular indeterminate size="64" color="primary" class="mb-4" />
            <p class="text-h6 text-center">Loading data...</p>
        </div>

        <!-- ===== ERROR STATE ===== -->
        <div v-else-if="HasError" class="error-container">
            <v-alert type="error" prominent class="mb-4">
                <template #title>
                    <div class="d-flex align-center">
                        <AlertCircle :size="24" class="mr-2" />
                        Error Loading Data
                    </div>
                </template>
                <p v-if="ErrorRecords">Records Error: {{ ErrorRecords }}</p>
                <p v-if="ErrorProduct">Product Error: {{ ErrorProduct }}</p>
                <v-btn @click="RetryLoadData" color="error" variant="outlined" class="mt-2">
                    <RefreshCw :size="18" class="mr-2" />
                    Retry
                </v-btn>
            </v-alert>
        </div>

        <!-- ===== MAIN CONTENT ===== -->
        <div v-else class="main-content">
            <!-- ===== HEADER CONTROLS ===== -->
            <div class="controls-section">
                <v-row justify="space-between" align="center" no-gutters class="mb-4">
                    <v-col cols="auto">
                        <h2 class="text-h4 font-weight-bold text-primary d-flex align-center">
                            <BarChart3 :size="28" class="mr-2" />
                            Thickness Chart Analysis
                        </h2>
                    </v-col>
                    <v-col cols="auto">
                        <div class="d-flex align-center gap-2">
                            <!-- Auto-Refresh Status -->
                            <v-chip :color="AutoRefreshEnabled ? 'success' : 'grey'" variant="flat" size="small"
                                class="px-3">
                                <Clock :size="14" class="mr-1" />
                                <template v-if="AutoRefreshEnabled">
                                    {{ FormatTimeRemaining(TimeUntilNextRefresh) }}
                                </template>
                                <template v-else>
                                    Off
                                </template>
                            </v-chip>

                            <!-- Download PDF Button -->
                            <v-btn @click="DownloadPDF" color="primary" size="large"
                                :disabled="LotChartList.length === 0" :loading="IsExporting" elevation="3" rounded="lg">
                                <FileDown :size="20" class="mr-2" />
                                Download PDF
                            </v-btn>
                        </div>
                    </v-col>
                </v-row>

                <!-- ===== AUTO-REFRESH CONTROLS ===== -->
                <v-card flat class="pa-4 mb-4 auto-refresh-card">
                    <v-row align="center" no-gutters>
                        <v-col cols="12" md="3">
                            <div class="d-flex align-center">
                                <v-switch v-model="AutoRefreshEnabled" @change="ToggleAutoRefresh" color="success"
                                    hide-details density="comfortable">
                                    <template #label>
                                        <div class="d-flex align-center">
                                            <component :is="AutoRefreshEnabled ? Power : PowerOff" :size="18"
                                                class="mr-2" :color="AutoRefreshEnabled ? '#4caf50' : '#9e9e9e'" />
                                            <span class="font-weight-medium">
                                                Auto-Refresh
                                            </span>
                                        </div>
                                    </template>
                                </v-switch>
                            </div>
                        </v-col>

                        <v-col cols="12" md="4">
                            <v-select v-model="RefreshInterval" :items="RefreshIntervalOptions"
                                @update:model-value="OnRefreshIntervalChange" label="Refresh Interval"
                                variant="outlined" density="comfortable" hide-details :disabled="!AutoRefreshEnabled"
                                rounded="lg">
                                <template #prepend-inner>
                                    <Clock :size="20" />
                                </template>
                            </v-select>
                        </v-col>

                        <v-col cols="12" md="3">
                            <v-btn @click="ManualRefresh" variant="outlined" color="primary" block
                                :loading="IsRefreshing" rounded="lg">
                                <RefreshCw :size="18" class="mr-2" :class="{ 'rotating': IsRefreshing }" />
                                Refresh Now
                            </v-btn>
                        </v-col>

                        <v-col cols="12" md="2">
                            <div v-if="LastRefreshTime" class="text-caption text-grey text-center">
                                Last: {{ format(LastRefreshTime, 'HH:mm:ss') }}
                            </div>
                        </v-col>
                    </v-row>
                </v-card>

                <!-- ===== FILTERS SECTION ===== -->
                <div v-if="FilteredSizes.length > 0" class="filters-section">
                    <v-card flat class="pa-4 filter-card">
                        <v-row align="center" no-gutters>
                            <!-- Product Size Filter -->
                            <v-col cols="12" md="3">
                                <v-select v-model="ImobileSize" :items="FilteredSizes" label="Product Size"
                                    variant="outlined" density="comfortable" clearable rounded="lg">
                                    <template #prepend-inner>
                                        <Package :size="20" color="#1976d2" />
                                    </template>
                                    <template #selection="{ item }">
                                        <v-chip size="small" color="primary" variant="flat">
                                            {{ item.title }}
                                        </v-chip>
                                    </template>
                                </v-select>
                            </v-col>

                            <!-- MC Filter -->
                            <v-col cols="12" md="3">
                                <v-select v-model="FilterMc" :items="McPoList" item-value="Value" item-title="Label"
                                    label="Machine Code" variant="outlined" density="comfortable" clearable class="ml-2"
                                    rounded="lg">
                                    <template #prepend-inner>
                                        <Settings :size="20" />
                                    </template>
                                </v-select>
                            </v-col>

                            <!-- Date Picker -->
                            <v-col cols="12" md="4">
                                <v-menu v-model="Menu" :close-on-content-click="false" transition="scale-transition">
                                    <template #activator="{ props }">
                                        <v-text-field v-bind="props" v-model="DisplayDate" label="Select Date"
                                            variant="outlined" density="comfortable" readonly class="ml-2" clearable
                                            @click:clear="ClearDateFilter" rounded="lg">
                                            <template #prepend-inner>
                                                <Calendar :size="20" />
                                            </template>
                                        </v-text-field>
                                    </template>
                                    <v-date-picker v-model="SelectedDate" @update:model-value="OnDateSelected"
                                        color="primary" elevation="8" />
                                </v-menu>
                            </v-col>

                            <!-- Clear All Filters -->
                            <v-col cols="12" md="2">
                                <v-btn @click="ClearAllFilters" variant="outlined" color="warning" class="ml-2" block
                                    rounded="lg">
                                    <FilterX :size="18" class="mr-2" />
                                    Clear All
                                </v-btn>
                            </v-col>
                        </v-row>
                    </v-card>
                </div>
            </div>

            <!-- ===== CHARTS CONTAINER ===== -->
            <div class="charts-container">
                <!-- Charts Info Bar -->
                <div v-if="LotChartList.length > 0" class="charts-info-bar">
                    <v-row align="center" no-gutters>
                        <v-col>
                            <div class="d-flex align-center">
                                <BarChart3 :size="24" color="#1976d2" class="mr-2" />
                                <span class="text-h6 font-weight-medium">
                                    {{ LotChartList.length }} Chart{{ LotChartList.length > 1 ? 's' : '' }} Found
                                </span>
                                <v-chip v-if="ImobileSize" color="primary" variant="flat" size="small" class="ml-3">
                                    {{ ImobileSize }}
                                </v-chip>
                            </div>
                        </v-col>
                        <v-col cols="auto">
                            <v-btn-toggle v-model="ViewMode" color="primary" variant="outlined" divided rounded="lg">
                                <v-btn value="horizontal" size="small">
                                    Horizontal
                                </v-btn>
                                <v-btn value="vertical" size="small">
                                    Vertical
                                </v-btn>
                            </v-btn-toggle>
                        </v-col>
                    </v-row>
                </div>

                <!-- ===== SEAMLESS CHARTS SCROLL ===== -->
                <div class="charts-scroll-wrapper">
                    <!-- Refreshing Indicator -->
                    <Transition name="slide-fade">
                        <div v-if="IsRefreshing" class="refresh-indicator">
                            <v-chip color="primary" variant="flat" size="small" class="px-3">
                                <RefreshCw :size="14" class="mr-1 rotating" />
                                Updating data...
                            </v-chip>
                        </div>
                    </Transition>

                    <TransitionGroup name="chart-list" tag="div" class="charts-scroll"
                        :class="{ 'vertical-layout': ViewMode === 'vertical' }">
                        <template v-for="(lotChart, index) in LotChartList" :key="`${lotChart.LotId}-${index}`">
                            <!-- Chart Item -->
                            <div class="chart-item-seamless" :ref="(el) => SetChartWrapperRef(el, index)">
                                <!-- Chart Header (clickable → opens Monitor Dialog) -->
                                <div class="chart-header-seamless chart-header-clickable"
                                    @click="OpenMonitorDialog(lotChart)"
                                    title="Click to open Thickness Measurement Monitor">
                                    <v-row no-gutters>
                                        <!-- Left: Product Info -->
                                        <v-col cols="6">
                                            <div class="product-info">
                                                <div class="text-h6 font-weight-bold text-primary mb-1 d-flex align-center gap-1">
                                                    {{ lotChart.Size }}
                                                    <v-icon size="14" color="primary" class="ml-1 header-open-icon">mdi-open-in-new</v-icon>
                                                </div>
                                                <div class="text-body-2 text-grey-darken-2 mb-1">
                                                    {{ lotChart.LotId }}
                                                </div>
                                                <div class="text-caption text-grey-darken-1">
                                                    {{ lotChart.PoLot }}
                                                </div>
                                            </div>
                                        </v-col>

                                        <!-- Right: Statistics -->
                                        <v-col cols="6">
                                            <div class="statistics-info text-right">
                                                <div class="text-body-2 font-weight-bold mb-1">
                                                    MAX: {{ FormatValue(lotChart.MaxValue || 0) }}
                                                </div>
                                                <div class="text-body-2 font-weight-bold mb-1">
                                                    MIN: {{ FormatValue(lotChart.MinValue || 0) }}
                                                </div>
                                                <div class="text-body-2 font-weight-bold">
                                                    AVG: {{ FormatValue(lotChart.AvgValue || 0) }}
                                                </div>
                                            </div>
                                        </v-col>
                                    </v-row>

                                    <!-- Status & Diff -->
                                    <v-row no-gutters class="mt-3">
                                        <v-col cols="8">
                                            <v-chip :color="GetStatusColor(lotChart.Status)" size="small"
                                                class="status-chip-seamless" variant="flat">
                                                <component :is="GetStatusIcon(lotChart.Status)" :size="16"
                                                    class="mr-1" />
                                                {{ lotChart.Status || 'Unknown' }}
                                            </v-chip>
                                        </v-col>
                                        <v-col cols="4" class="text-right">
                                            <v-chip color="info" size="small" variant="flat">
                                                Δ {{ Math.round(((lotChart.MaxValue || 0) - (lotChart.MinValue || 0)) *
                                                1000) }}
                                            </v-chip>
                                        </v-col>
                                    </v-row>
                                </div>

                                <!-- Chart Content -->
                                <div class="chart-content-seamless">
                                    <Scatter v-if="lotChart.Datasets && lotChart.Datasets.length > 0" :data="{
                                        datasets: lotChart.Datasets.map(d => ({
                                            label: d.Label,
                                            data: d.Data.map(point => ({ x: point.X, y: point.Y })),
                                            backgroundColor: d.BackgroundColor,
                                            pointRadius: d.PointRadius,
                                            pointHoverRadius: d.PointHoverRadius,
                                            showLine: d.ShowLine
                                        }))
                                    }"
                                        :options="GetChartOptions(lotChart.Size, lotChart.MinValue, lotChart.MaxValue)" />
                                </div>

                                <!-- Chart Footer -->
                                <div class="chart-footer-seamless">
                                    <v-btn @click="ExportSingleChart(index)" size="x-small" variant="text"
                                        color="primary">
                                        <Download :size="14" class="mr-1" />
                                        Export
                                    </v-btn>
                                </div>
                            </div>

                            <!-- Vertical Divider (not last item) -->
                            <div v-if="index < LotChartList.length - 1" class="chart-divider-seamless"
                                :key="`divider-${index}`">
                                <div class="divider-line"></div>
                            </div>
                        </template>
                    </TransitionGroup>
                </div>

                <!-- ===== EMPTY STATE ===== -->
                <div v-if="LotChartList.length === 0" class="empty-state">
                    <v-card flat class="text-center pa-8">
                        <Package :size="64" color="#BDBDBD" class="mb-4" />
                        <h3 class="text-h5 font-weight-medium mb-2">No Charts to Display</h3>
                        <p class="text-body-1 text-grey">
                            No data matches your current filter criteria.
                        </p>
                    </v-card>
                </div>
            </div>
        </div>

        <!-- ===== EXPORT PROGRESS DIALOG ===== -->
        <v-dialog v-model="ShowExportDialog" max-width="400" persistent>
            <v-card>
                <v-card-title class="text-h6">
                    <FileDown :size="20" class="mr-2" />
                    Exporting PDF
                </v-card-title>
                <v-card-text>
                    <v-progress-linear :model-value="ExportProgress" color="primary" height="8" rounded />
                    <p class="text-body-2 text-center mt-2">
                        {{ Math.ceil(ExportProgress) }}% complete
                    </p>
                </v-card-text>
            </v-card>
        </v-dialog>

        <!-- ===== THICKNESS MONITOR DIALOG ===== -->
        <ThicknessMonitorDialog
            v-model="ShowMonitorDialog"
            :lot-data="SelectedLotChart"
            :thresholds="SelectedLotChart ? GetThresholdsForSize(SelectedLotChart.Size) : []"
        />
    </div>
</template>

<!-- ====================================== -->
<!-- STYLE SECTION -->
<!-- ====================================== -->

<style scoped>
/* ====================================== */
/* MAIN CONTAINER */
/* ====================================== */
.thickness-chart-container {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    min-height: 100vh;
    padding: 20px;
}

.main-content {
    background: rgba(255, 255, 255, 0.98);
    border-radius: 24px;
    padding: 32px;
    backdrop-filter: blur(10px);
    min-height: calc(100vh - 40px);
    box-shadow: 0 20px 60px rgba(0, 0, 0, 0.15);
}

/* ====================================== */
/* CONTROLS & FILTERS */
/* ====================================== */
.controls-section {
    margin-bottom: 24px;
}

.auto-refresh-card {
    background: linear-gradient(135deg, #e8f5e9 0%, #c8e6c9 100%);
    border-radius: 16px;
    border: 1px solid rgba(76, 175, 80, 0.3);
    box-shadow: 0 2px 8px rgba(76, 175, 80, 0.1);
}

.filter-card {
    background: linear-gradient(135deg, #f8fafc 0%, #e2e8f0 100%);
    border-radius: 16px;
    border: 1px solid rgba(226, 232, 240, 0.8);
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
}

/* ====================================== */
/* CHARTS CONTAINER */
/* ====================================== */
.charts-container {
    display: flex;
    flex-direction: column;
    flex: 1;
    overflow: hidden;
}

.charts-info-bar {
    background: linear-gradient(135deg, #f8fafc 0%, #e2e8f0 100%);
    border-radius: 12px;
    padding: 16px;
    margin-bottom: 16px;
    border: 1px solid rgba(226, 232, 240, 0.8);
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
}

.charts-scroll-wrapper {
    flex: 1;
    overflow: hidden;
    background: white;
    border-radius: 16px;
    border: 2px solid rgba(226, 232, 240, 0.8);
    box-shadow: 0 4px 16px rgba(0, 0, 0, 0.08);
    position: relative;
}

/* ====================================== */
/* REFRESH INDICATOR */
/* ====================================== */
.refresh-indicator {
    position: absolute;
    top: 16px;
    right: 16px;
    z-index: 10;
    animation: slideInDown 0.3s ease-out;
}

@keyframes rotating {
    from {
        transform: rotate(0deg);
    }

    to {
        transform: rotate(360deg);
    }
}

.rotating {
    animation: rotating 1s linear infinite;
}

/* ====================================== */
/* SEAMLESS CHARTS LAYOUT */
/* ====================================== */
.charts-scroll {
    display: flex;
    flex-direction: row;
    overflow-x: auto;
    overflow-y: hidden;
    height: 100%;
    padding: 0;
    gap: 0;
}

.charts-scroll.vertical-layout {
    flex-direction: column;
    overflow-x: hidden;
    overflow-y: auto;
}

/* ====================================== */
/* SEAMLESS CHART ITEM */
/* ====================================== */
.chart-item-seamless {
    flex: 0 0 400px;
    min-width: 400px;
    max-width: 400px;
    display: flex;
    flex-direction: column;
    background: white;
    transition: background-color 0.2s ease;
}

.chart-item-seamless:hover {
    background: rgba(248, 250, 252, 0.5);
}

.vertical-layout .chart-item-seamless {
    flex: 0 0 auto;
    min-width: 100%;
    max-width: 100%;
}

/* ====================================== */
/* CHART SECTIONS */
/* ====================================== */
.chart-header-seamless {
    padding: 16px;
    background: linear-gradient(135deg, #f8fafc 0%, #ffffff 100%);
    border-bottom: 1px solid rgba(226, 232, 240, 0.6);
}

/* Clickable header variant */
.chart-header-clickable {
    cursor: pointer;
    transition: background 0.18s ease, box-shadow 0.18s ease;
    user-select: none;
    position: relative;
}

.chart-header-clickable:hover {
    background: linear-gradient(135deg, #e8f0fe 0%, #f0f6ff 100%);
    box-shadow: 0 2px 8px rgba(25, 118, 210, 0.12);
}

.chart-header-clickable:hover .header-open-icon {
    opacity: 1;
    transform: scale(1.15);
}

.header-open-icon {
    opacity: 0.45;
    transition: opacity 0.18s, transform 0.18s;
}

.chart-content-seamless {
    padding: 1.25rem 1rem;
    height: 50rem;
    flex: 1;
}

.chart-footer-seamless {
    padding: 8px 16px;
    text-align: center;
    border-top: 1px solid rgba(226, 232, 240, 0.6);
    background: linear-gradient(135deg, #ffffff 0%, #f8fafc 100%);
}

.chart-content-seamless canvas {
    min-height: 400px !important;
    height: 100% !important;
}

/* ====================================== */
/* DIVIDER */
/* ====================================== */
.chart-divider-seamless {
    flex: 0 0 1px;
    display: flex;
    align-items: stretch;
    background: transparent;
}

.divider-line {
    width: 1px;
    background: linear-gradient(to bottom,
            rgba(226, 232, 240, 0) 0%,
            rgba(226, 232, 240, 0.8) 10%,
            rgba(226, 232, 240, 0.8) 90%,
            rgba(226, 232, 240, 0) 100%);
}

.vertical-layout .chart-divider-seamless {
    flex: 0 0 1px;
    width: 100%;
    height: 1px;
}

.vertical-layout .divider-line {
    width: 100%;
    height: 1px;
    background: linear-gradient(to right,
            rgba(226, 232, 240, 0) 0%,
            rgba(226, 232, 240, 0.8) 10%,
            rgba(226, 232, 240, 0.8) 90%,
            rgba(226, 232, 240, 0) 100%);
}

/* ====================================== */
/* STATUS CHIP */
/* ====================================== */
.status-chip-seamless {
    font-weight: 600 !important;
    text-transform: uppercase;
    letter-spacing: 0.5px;
    color: white !important;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

/* ====================================== */
/* SCROLLBAR STYLING */
/* ====================================== */
.charts-scroll::-webkit-scrollbar {
    width: 8px;
    height: 8px;
}

.charts-scroll::-webkit-scrollbar-track {
    background: #f1f5f9;
    border-radius: 4px;
}

.charts-scroll::-webkit-scrollbar-thumb {
    background: linear-gradient(45deg, #667eea, #764ba2);
    border-radius: 4px;
}

.charts-scroll::-webkit-scrollbar-thumb:hover {
    background: linear-gradient(45deg, #5a6fd8, #6b4190);
}

/* ====================================== */
/* EMPTY STATE */
/* ====================================== */
.empty-state {
    flex: 1;
    display: flex;
    align-items: center;
    justify-content: center;
    min-height: 400px;
}

/* ====================================== */
/* RESPONSIVE */
/* ====================================== */
@media (max-width: 768px) {
    .thickness-chart-container {
        padding: 12px;
    }

    .main-content {
        padding: 20px;
    }

    .chart-item-seamless {
        min-width: 340px;
        max-width: 340px;
    }

    .chart-content-seamless {
        height: 31.25rem;
    }
}

@media (max-width: 480px) {
    .chart-item-seamless {
        min-width: 300px;
        max-width: 300px;
    }

    .chart-content-seamless {
        height: 28rem;
        padding: 1rem 0.75rem;
    }
}

/* ====================================== */
/* ANIMATIONS */
/* ====================================== */
@keyframes slideIn {
    from {
        opacity: 0;
        transform: translateX(-20px);
    }

    to {
        opacity: 1;
        transform: translateX(0);
    }
}

@keyframes slideInDown {
    from {
        opacity: 0;
        transform: translateY(-20px);
    }

    to {
        opacity: 1;
        transform: translateY(0);
    }
}

.chart-item-seamless {
    animation: slideIn 0.3s ease-out;
}

/* ====================================== */
/* TRANSITION ANIMATIONS */
/* ====================================== */
.slide-fade-enter-active {
    transition: all 0.3s ease-out;
}

.slide-fade-leave-active {
    transition: all 0.2s ease-in;
}

.slide-fade-enter-from {
    transform: translateY(-20px);
    opacity: 0;
}

.slide-fade-leave-to {
    transform: translateY(-20px);
    opacity: 0;
}

/* Chart List Transitions */
.chart-list-move,
.chart-list-enter-active {
    transition: all 0.4s ease;
}

.chart-list-enter-from {
    opacity: 0;
    transform: translateX(-30px);
}

.chart-list-leave-active {
    position: absolute;
    transition: all 0.3s ease;
}

.chart-list-leave-to {
    opacity: 0;
    transform: translateX(30px);
}

/* ====================================== */
/* LOADING & ERROR STATES */
/* ====================================== */
.loading-container,
.error-container {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    min-height: 60vh;
    padding: 40px;
}

@keyframes pulse {

    0%,
    100% {
        opacity: 1;
    }

    50% {
        opacity: 0.7;
    }
}

.loading-container {
    animation: pulse 2s infinite;
}

/* ====================================== */
/* UTILITY CLASSES */
/* ====================================== */
.gap-2 {
    gap: 8px;
}
</style>
