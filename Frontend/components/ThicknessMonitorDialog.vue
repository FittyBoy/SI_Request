<script setup lang="ts">
// ====================================== 
// THICKNESS MEASUREMENT MONITOR DIALOG
// ====================================== 
import { computed, ref, onMounted, onUnmounted } from 'vue'
import { Bar } from 'vue-chartjs'
import {
    Chart as ChartJS,
    BarController,
    CategoryScale,
    LinearScale,
    BarElement,
    LineController,
    LineElement,
    PointElement,
    Filler,
    Tooltip,
    Legend,
} from 'chart.js'
import annotationPlugin from 'chartjs-plugin-annotation'
import { X } from 'lucide-vue-next'

ChartJS.register(
    BarController, CategoryScale, LinearScale, BarElement,
    LineController, LineElement, PointElement, Filler,
    Tooltip, Legend, annotationPlugin
)

// ===== PROPS & EMITS =====
interface Threshold {
    Value: number
    Label: string
    Color: string
}

interface Props {
    modelValue: boolean
    lotData: any | null
    thresholds: Threshold[]
}

const props = defineProps<Props>()
const emit = defineEmits(['update:modelValue'])
const close = () => emit('update:modelValue', false)

// ===== LIVE CLOCK =====
const currentTime = ref('')
let clockTimer: ReturnType<typeof setInterval> | null = null

const updateClock = () => {
    const now = new Date()
    const hh = String(now.getHours()).padStart(2, '0')
    const mm = String(now.getMinutes()).padStart(2, '0')
    const ss = String(now.getSeconds()).padStart(2, '0')
    currentTime.value = `${hh}:${mm}:${ss}`
}

onMounted(() => { updateClock(); clockTimer = setInterval(updateClock, 1000) })
onUnmounted(() => { if (clockTimer) clearInterval(clockTimer) })

// ===== THRESHOLD HELPERS =====
const getThresholdByLabel = (label: string): number => {
    const t = props.thresholds.find(t => t.Label === label)
    return t?.Value ?? 0
}

const USL = computed(() => getThresholdByLabel('Hold'))
const Target = computed(() => getThresholdByLabel('Targetbar'))
const RescreenUpper = computed(() => {
    const rs = props.thresholds.filter(t => t.Label === 'Re-screen')
    return rs.length > 0 ? Math.max(...rs.map(t => t.Value)) : USL.value
})
const RescreenLower = computed(() => {
    const rs = props.thresholds.filter(t => t.Label === 'Re-screen')
    return rs.length > 1 ? Math.min(...rs.map(t => t.Value)) : Target.value - (USL.value - Target.value)
})
const LSL = computed(() => Target.value - (USL.value - Target.value))

// ===== RAW MEASUREMENT ARRAYS =====
const carriers = ['Ca1', 'Ca2', 'Ca3', 'Ca4', 'Ca5']

const getArr = (key: string): (number | null)[] => props.lotData?.[key] || []

const caInArrays = computed(() => carriers.map(ca => getArr(ca + 'In')))
const caOutArrays = computed(() => carriers.map(ca => getArr(ca + 'Out')))

const allOutValues = computed(() =>
    caOutArrays.value.flat().filter((v): v is number => v !== null && v !== undefined)
)
const allInValues = computed(() =>
    caInArrays.value.flat().filter((v): v is number => v !== null && v !== undefined)
)
const allValues = computed(() => [...allInValues.value, ...allOutValues.value])

// ===== STATISTICS =====
const mean = (arr: number[]): number =>
    arr.length ? arr.reduce((a, b) => a + b, 0) / arr.length : 0

const stdev = (arr: number[]): number => {
    if (arr.length < 2) return 0
    const m = mean(arr)
    return Math.sqrt(arr.reduce((s, v) => s + (v - m) ** 2, 0) / (arr.length - 1))
}

const sampleMean = computed(() => mean(allValues.value))
const stdevOverall = computed(() => stdev(allValues.value))
const stdevWithin = computed(() => {
    const subStdevs: number[] = []
    carriers.forEach((_, i) => {
        const inVals = caInArrays.value[i].filter((v): v is number => v !== null)
        const outVals = caOutArrays.value[i].filter((v): v is number => v !== null)
        if (inVals.length > 1) subStdevs.push(stdev(inVals))
        if (outVals.length > 1) subStdevs.push(stdev(outVals))
    })
    if (!subStdevs.length) return stdevOverall.value
    return Math.sqrt(subStdevs.reduce((s, v) => s + v * v, 0) / subStdevs.length)
})

const Cp = computed(() => stdevWithin.value > 0 ? (USL.value - LSL.value) / (6 * stdevWithin.value) : 0)
const Cpl = computed(() => stdevWithin.value > 0 ? (sampleMean.value - LSL.value) / (3 * stdevWithin.value) : 0)
const Cpu = computed(() => stdevWithin.value > 0 ? (USL.value - sampleMean.value) / (3 * stdevWithin.value) : 0)
const Cpk = computed(() => Math.min(Cpl.value, Cpu.value))
const Pp = computed(() => stdevOverall.value > 0 ? (USL.value - LSL.value) / (6 * stdevOverall.value) : 0)
const Ppl = computed(() => stdevOverall.value > 0 ? (sampleMean.value - LSL.value) / (3 * stdevOverall.value) : 0)
const Ppu = computed(() => stdevOverall.value > 0 ? (USL.value - sampleMean.value) / (3 * stdevOverall.value) : 0)
const Ppk = computed(() => Math.min(Ppl.value, Ppu.value))
const CCpk = computed(() => Cpk.value)
const Cpm = computed(() => {
    if (stdevWithin.value === 0) return 0
    const t = (sampleMean.value - Target.value) / stdevWithin.value
    return Cp.value / Math.sqrt(1 + t * t)
})

const fmt = (v: number, d = 3) => (isNaN(v) || !isFinite(v)) ? 'N/A' : v.toFixed(d)

// ===== DERIVED METRICS =====
const maxValue = computed(() => allValues.value.length ? Math.max(...allValues.value) : 0)
const minValue = computed(() => allValues.value.length ? Math.min(...allValues.value) : 0)
const differenceNm = computed(() => Math.round((maxValue.value - minValue.value) * 1000))
const removalNm = computed(() => {
    const thBefore = props.lotData?.ThBefore ?? props.lotData?.thBefore
    if (thBefore && sampleMean.value) return Math.round((thBefore - sampleMean.value) * 1000)
    return props.lotData?.Margin ?? props.lotData?.margin ?? '-'
})
const rate = computed(() => props.lotData?.Rate ?? props.lotData?.poRate ?? props.lotData?.PoRate ?? '-')

// ===== LOT INFO FIELDS =====
const lotNumber = computed(() => props.lotData?.LotId ?? props.lotData?.imobileLot ?? '-')
const polLotParts = computed(() => {
    const lp = props.lotData?.LotPo ?? props.lotData?.lotPo ?? ''
    const mc = props.lotData?.McPo ?? props.lotData?.mcPo ?? ''
    const no = props.lotData?.NoPo ?? props.lotData?.noPo ?? ''
    return { lp, mc, no }
})
const productSize = computed(() => props.lotData?.Size ?? props.lotData?.imobileSize ?? '-')
const process = computed(() => props.lotData?.Process ?? props.lotData?.process ?? '-')
const mc = computed(() => props.lotData?.McPo ?? props.lotData?.mcPo ?? '-')
const thBefore = computed(() => {
    const v = props.lotData?.ThBefore ?? props.lotData?.thBefore
    return v != null ? Number(v).toFixed(3) : '-'
})
const processingTime = computed(() => props.lotData?.ProcessTime ?? props.lotData?.processTime ?? '-')
const cassetteNo = computed(() => {
    const lp = props.lotData?.LotPo ?? props.lotData?.lotPo ?? ''
    const mc = props.lotData?.McPo ?? props.lotData?.mcPo ?? ''
    return lp && mc ? `${lp}-${mc}` : '-'
})
const programNo = computed(() => props.lotData?.Program ?? props.lotData?.program ?? '0')
const result = computed(() => props.lotData?.Result ?? props.lotData?.result ?? props.lotData?.Status ?? '-')

const resultColor = computed(() => {
    const r = result.value?.toLowerCase() ?? ''
    if (r === 'ok') return '#2e7d32'
    if (r === 'hold') return '#1565c0'
    if (r === 'scrap') return '#c62828'
    if (r.includes('rescreen') || r.includes('re-screen')) return '#e65100'
    return '#555'
})

// ===== CELL COLOR (for measurement grid) =====
const getCellStyle = (value: number | null) => {
    if (value === null || value === undefined) return { background: '#e0e0e0', color: '#888', border: '1.5px solid #bbb' }
    const sorted = [...props.thresholds].sort((a, b) => b.Value - a.Value)

    const bgMap: Record<string, string> = {
        blue: '#bbdefb',
        orange: '#ffe0b2',
        green: '#c8e6c9',
        red: '#ffcdd2',
    }
    const borderMap: Record<string, string> = {
        blue: '#1976d2',
        orange: '#f57c00',
        green: '#388e3c',
        red: '#d32f2f',
    }

    for (const t of sorted) {
        if (value >= t.Value) {
            return {
                background: bgMap[t.Color] || '#e0e0e0',
                color: borderMap[t.Color] || '#333',
                border: `1.5px solid ${borderMap[t.Color] || '#aaa'}`,
            }
        }
    }

    const last = sorted[sorted.length - 1]
    return {
        background: bgMap[last?.Color] || '#ffcdd2',
        color: borderMap[last?.Color] || '#d32f2f',
        border: `1.5px solid ${borderMap[last?.Color] || '#d32f2f'}`,
    }
}

// 5-point layout: [top-left, top-right, center, bot-left, bot-right]
const layout5 = [
    [0, 1],  // row 1: indices 0 and 1
    [2],     // row 2: index 2 (center)
    [3, 4],  // row 3: indices 3 and 4
]

const getPointValue = (arr: (number | null)[], idx: number): number | null => {
    return arr?.[idx] ?? null
}

// ===== HISTOGRAM (Cpk chart) =====
const histBuckets = computed(() => {
    if (!allValues.value.length) return { labels: [], counts: [], centers: [] }
    const lo = Math.min(LSL.value - 0.002, Math.min(...allValues.value) - 0.002)
    const hi = Math.max(USL.value + 0.002, Math.max(...allValues.value) + 0.002)
    const step = 0.002
    const labels: string[] = []
    const counts: number[] = []
    const centers: number[] = []
    for (let v = lo; v < hi; v += step) {
        labels.push(v.toFixed(3))
        centers.push(v + step / 2)
        counts.push(allValues.value.filter(x => x >= v && x < v + step).length)
    }
    return { labels, counts, centers }
})

const normalPDF = (x: number, mu: number, sigma: number): number => {
    if (sigma === 0) return 0
    return (1 / (sigma * Math.sqrt(2 * Math.PI))) * Math.exp(-0.5 * ((x - mu) / sigma) ** 2)
}

const cpkChartData = computed(() => {
    const { labels, counts, centers } = histBuckets.value
    if (!labels.length) return { labels: [], datasets: [] }

    const maxCount = Math.max(...counts, 1)
    const step = 0.002
    const n = allValues.value.length

    // Scale normal curve to match histogram height
    const withinCurve = centers.map(c => {
        const density = normalPDF(c, sampleMean.value, stdevWithin.value)
        return density * n * step * (maxCount / (n * step * normalPDF(sampleMean.value, sampleMean.value, stdevWithin.value) || 1))
    })

    const overallCurve = centers.map(c => {
        const density = normalPDF(c, sampleMean.value, stdevOverall.value)
        return density * n * step * (maxCount / (n * step * normalPDF(sampleMean.value, sampleMean.value, stdevOverall.value) || 1))
    })

    return {
        labels,
        datasets: [
            {
                type: 'bar' as const,
                label: 'Frequency',
                data: counts,
                backgroundColor: 'rgba(33, 150, 243, 0.65)',
                borderColor: '#1565c0',
                borderWidth: 1,
                order: 2,
            },
            {
                type: 'line' as const,
                label: 'Within Fit',
                data: withinCurve,
                borderColor: '#111',
                borderWidth: 2,
                borderDash: [5, 3],
                pointRadius: 0,
                fill: false,
                order: 1,
            },
            {
                type: 'line' as const,
                label: 'Overall Fit',
                data: overallCurve,
                borderColor: '#e53935',
                borderWidth: 2,
                borderDash: [5, 3],
                pointRadius: 0,
                fill: false,
                order: 0,
            },
        ] as any,
    }
})

const cpkAnnotations = computed(() => {
    const { labels } = histBuckets.value
    if (!labels.length) return {}

    const toIndex = (val: number) => {
        const step = 0.002
        const lo = parseFloat(labels[0])
        return (val - lo) / step
    }

    return {
        lslLine: {
            type: 'line',
            xMin: toIndex(LSL.value),
            xMax: toIndex(LSL.value),
            borderColor: '#e53935',
            borderWidth: 2,
            borderDash: [6, 3],
            label: { content: 'LSL', display: true, position: 'start', color: '#e53935', font: { size: 10 } },
        },
        targetLine: {
            type: 'line',
            xMin: toIndex(Target.value),
            xMax: toIndex(Target.value),
            borderColor: '#388e3c',
            borderWidth: 2,
            borderDash: [6, 3],
            label: { content: 'T', display: true, position: 'start', color: '#388e3c', font: { size: 10 } },
        },
        uslLine: {
            type: 'line',
            xMin: toIndex(USL.value),
            xMax: toIndex(USL.value),
            borderColor: '#1565c0',
            borderWidth: 2,
            borderDash: [6, 3],
            label: { content: 'USL', display: true, position: 'start', color: '#1565c0', font: { size: 10 } },
        },
    }
})

const cpkChartOptions = computed(() => ({
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
        legend: {
            display: true,
            position: 'top' as const,
            labels: { font: { size: 10 }, boxWidth: 20 },
        },
        annotation: { annotations: cpkAnnotations.value },
        tooltip: { enabled: false },
    },
    scales: {
        x: {
            ticks: {
                font: { size: 9 },
                maxRotation: 45,
                callback: (_: any, i: number) => {
                    const { labels } = histBuckets.value
                    return i % 2 === 0 ? labels[i] : ''
                },
            },
            grid: { display: false },
        },
        y: {
            beginAtZero: true,
            ticks: { font: { size: 9 }, stepSize: 1 },
            grid: { color: 'rgba(0,0,0,0.07)' },
        },
    },
}))

// ===== TREND CHART =====
const trendChartData = computed(() => {
    const xLabels = carriers.flatMap((_, i) => [`C${i + 1} IN`, `C${i + 1} OUT`])

    const inDatasets = carriers.map((_, i) => {
        const vals = caInArrays.value[i].filter((v): v is number => v !== null)
        return vals.map(v => ({ x: i * 2, y: v }))
    })
    const outDatasets = carriers.map((_, i) => {
        const vals = caOutArrays.value[i].filter((v): v is number => v !== null)
        return vals.map(v => ({ x: i * 2 + 1, y: v }))
    })

    const allPoints = [...inDatasets.flat(), ...outDatasets.flat()]

    // Average per position for trend line
    const avgPoints = xLabels.map((_, xi) => {
        const pts = allPoints.filter(p => p.x === xi).map(p => p.y)
        return { x: xi, y: pts.length ? mean(pts) : null }
    }).filter(p => p.y !== null)

    return {
        labels: xLabels,
        datasets: [
            {
                type: 'scatter' as const,
                label: 'Measurements',
                data: allPoints,
                backgroundColor: '#2196f3',
                pointRadius: 5,
                order: 2,
            },
            {
                type: 'line' as const,
                label: 'Trend',
                data: avgPoints,
                borderColor: '#1565c0',
                borderWidth: 1.5,
                pointRadius: 0,
                fill: false,
                tension: 0,
                order: 1,
            },
        ] as any,
    }
})

const trendAnnotations = computed(() => ({
    holdLine: {
        type: 'line', yMin: USL.value, yMax: USL.value,
        borderColor: '#1565c0', borderWidth: 1.5,
        label: { content: 'Hold', display: true, position: 'end', font: { size: 9 }, color: '#1565c0' },
    },
    rescreenLine: {
        type: 'line', yMin: RescreenUpper.value, yMax: RescreenUpper.value,
        borderColor: '#f57c00', borderWidth: 1.5,
        label: { content: 'Re-scr', display: true, position: 'end', font: { size: 9 }, color: '#f57c00' },
    },
    targetLine: {
        type: 'line', yMin: Target.value, yMax: Target.value,
        borderColor: '#388e3c', borderWidth: 1.5, borderDash: [4, 3],
        label: { content: 'Target', display: true, position: 'end', font: { size: 9 }, color: '#388e3c' },
    },
    rescreenLowLine: {
        type: 'line', yMin: RescreenLower.value, yMax: RescreenLower.value,
        borderColor: '#f57c00', borderWidth: 1.5,
    },
    lslLine: {
        type: 'line', yMin: LSL.value, yMax: LSL.value,
        borderColor: '#d32f2f', borderWidth: 1.5,
    },
}))

const trendChartOptions = computed(() => {
    const allVals = allValues.value
    const yMin = allVals.length ? Math.min(...allVals, LSL.value) - 0.003 : 0.180
    const yMax = allVals.length ? Math.max(...allVals, USL.value) + 0.003 : 0.220

    return {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            legend: { display: false },
            annotation: { annotations: trendAnnotations.value },
            tooltip: {
                callbacks: {
                    label: (ctx: any) => `${Number(ctx.parsed.y).toFixed(3)} μm`,
                },
            },
        },
        scales: {
            x: {
                type: 'linear' as const,
                min: -0.5,
                max: 9.5,
                ticks: {
                    stepSize: 1,
                    font: { size: 9 },
                    maxRotation: 45,
                    callback: (_: any, i: number) => {
                        const labels = ['C1 IN', 'C1 OUT', 'C2 IN', 'C2 OUT', 'C3 IN', 'C3 OUT', 'C4 IN', 'C4 OUT', 'C5 IN', 'C5 OUT']
                        return labels[i] || ''
                    },
                },
                grid: { color: 'rgba(0,0,0,0.07)' },
            },
            y: {
                min: yMin,
                max: yMax,
                ticks: {
                    font: { size: 9 },
                    callback: (v: any) => Number(v).toFixed(3),
                    stepSize: 0.005,
                },
                grid: { color: 'rgba(0,0,0,0.07)' },
            },
        },
    }
})
</script>

<!-- ====================================== -->
<!-- TEMPLATE -->
<!-- ====================================== -->
<template>
    <v-dialog :model-value="modelValue" @update:model-value="emit('update:modelValue', $event)" fullscreen
        transition="dialog-bottom-transition" scrollable>
        <div class="monitor-wrapper" v-if="lotData">

            <!-- ===== TITLE BAR ===== -->
            <div class="monitor-titlebar">
                <span class="monitor-title">Thickness Measurement Monitor</span>
                <div class="titlebar-right">
                    <div class="ready-chip">
                        <span class="ready-dot"></span>
                        READY
                    </div>
                    <span class="monitor-clock">{{ currentTime }}</span>
                    <button class="close-btn" @click="close">
                        <X :size="18" />
                    </button>
                </div>
            </div>

            <!-- ===== BODY ===== -->
            <div class="monitor-body">

                <!-- LEFT PANEL -->
                <div class="left-panel">
                    <div class="field-group">
                        <label class="field-label">Lot #</label>
                        <div class="field-input">{{ lotNumber }}</div>
                    </div>

                    <button class="connect-btn">Connect</button>

                    <div class="field-group">
                        <label class="field-label">Polishing Lot no</label>
                        <div class="pol-lot-row">
                            <div class="field-input field-small">{{ polLotParts.lp }}</div>
                            <span class="lot-sep">-</span>
                            <div class="field-input field-small">{{ polLotParts.mc }}</div>
                            <span class="lot-sep">-</span>
                            <div class="field-input field-small">{{ polLotParts.no }}</div>
                        </div>
                    </div>

                    <div class="field-group">
                        <label class="field-label">Product Size</label>
                        <div class="field-input">{{ productSize }}</div>
                    </div>

                    <div class="field-group">
                        <label class="field-label">Process</label>
                        <div class="field-input">{{ process }}</div>
                    </div>

                    <div class="field-group">
                        <label class="field-label">MC</label>
                        <div class="field-input">{{ mc }}</div>
                    </div>

                    <div class="field-group">
                        <label class="field-label">Thickness before</label>
                        <div class="field-input">{{ thBefore }}</div>
                    </div>

                    <div class="field-group">
                        <label class="field-label">Processing Time (min)</label>
                        <div class="field-input">{{ processingTime }}</div>
                    </div>

                    <div class="field-group">
                        <label class="field-label">Cassette no</label>
                        <div class="field-input">{{ cassetteNo }}</div>
                    </div>

                    <div class="field-group">
                        <label class="field-label">Program no</label>
                        <div class="field-input">{{ programNo }}</div>
                    </div>

                    <div class="result-badge" :style="{ background: resultColor }">
                        <span class="result-label">RESULT</span>
                        <span class="result-value">{{ result }}</span>
                    </div>
                </div>

                <!-- CENTER PANEL -->
                <div class="center-panel">

                    <!-- MONITOR GRID -->
                    <div class="monitor-section">
                        <div class="section-header">Monitor</div>

                        <!-- IN Row -->
                        <div class="grid-row">
                            <div class="row-label-badge in-badge">IN</div>
                            <div v-for="(caIn, ci) in caInArrays" :key="`in-${ci}`" class="cassette-block">
                                <template v-for="(row, ri) in layout5" :key="`r${ri}`">
                                    <div class="wafer-row" :class="{ 'single-cell': row.length === 1 }">
                                        <div v-for="idx in row" :key="idx" class="wafer-cell"
                                            :style="getCellStyle(getPointValue(caIn, idx))">
                                            {{
                                                getPointValue(caIn, idx) != null
                                                    ? Number(getPointValue(caIn, idx)).toFixed(3)
                                                    : '-'
                                            }}
                                        </div>
                                    </div>
                                </template>
                            </div>
                        </div>

                        <!-- OUT Row -->
                        <div class="grid-row" style="margin-top:4px">
                            <div class="row-label-badge out-badge">OUT</div>
                            <div v-for="(caOut, ci) in caOutArrays" :key="`out-${ci}`" class="cassette-block">
                                <template v-for="(row, ri) in layout5" :key="`r${ri}`">
                                    <div class="wafer-row" :class="{ 'single-cell': row.length === 1 }">
                                        <div v-for="idx in row" :key="idx" class="wafer-cell"
                                            :style="getCellStyle(getPointValue(caOut, idx))">
                                            {{
                                                getPointValue(caOut, idx) != null
                                                    ? Number(getPointValue(caOut, idx)).toFixed(3)
                                                    : '-'
                                            }}
                                        </div>
                                    </div>
                                </template>
                            </div>
                        </div>
                    </div>

                    <!-- CHARTS ROW -->
                    <div class="charts-row">
                        <!-- Thickness Trend -->
                        <div class="chart-box">
                            <div class="chart-box-header">
                                <span>Thickness Trend</span>
                                <span class="chart-unit">μm</span>
                            </div>
                            <div class="chart-area">
                                <Bar :data="trendChartData as any" :options="trendChartOptions as any" />
                            </div>
                        </div>

                        <!-- Process Capability -->
                        <div class="chart-box">
                            <div class="chart-box-header">
                                <span>Process Capability (Cpk)</span>
                                <span class="spec-info">
                                    LSL: {{ LSL.toFixed(3) }} | Target: {{ Target.toFixed(3) }} | USL: {{
                                        USL.toFixed(3) }}
                                </span>
                            </div>
                            <div class="chart-area">
                                <Bar :data="cpkChartData as any" :options="cpkChartOptions as any" />
                            </div>
                        </div>
                    </div>
                </div>

                <!-- RIGHT PANEL -->
                <div class="right-panel">
                    <div class="right-section-title">Averages &amp; Metrics</div>

                    <!-- Averages -->
                    <div class="metrics-group">
                        <div class="metrics-group-label">AVERAGES</div>
                        <div class="metric-row">
                            <span>Average</span>
                            <span class="metric-val">{{ fmt(sampleMean) }}</span>
                        </div>
                        <div class="metric-row">
                            <span>Rate</span>
                            <span class="metric-val">{{ typeof rate === 'number' ? fmt(rate) : rate }}</span>
                        </div>
                        <div class="metric-row">
                            <span>Difference</span>
                            <span class="metric-val diff-val">{{ differenceNm }}</span>
                        </div>
                        <div class="metric-row">
                            <span>Removal</span>
                            <span class="metric-val">{{ removalNm }}</span>
                        </div>
                    </div>

                    <!-- Extremes -->
                    <div class="metrics-group">
                        <div class="metrics-group-label">EXTREMES</div>
                        <div class="metric-row">
                            <span>Max Value</span>
                            <span class="metric-val">{{ fmt(maxValue) }}</span>
                        </div>
                        <div class="metric-row">
                            <span>Min Value</span>
                            <span class="metric-val">{{ fmt(minValue) }}</span>
                        </div>
                    </div>

                    <div class="right-divider"></div>

                    <!-- Process Data -->
                    <div class="metrics-group">
                        <div class="metrics-group-label">PROCESS DATA</div>
                        <div class="metric-row">
                            <span>LSL</span>
                            <span class="metric-val">{{ fmt(LSL) }}</span>
                        </div>
                        <div class="metric-row">
                            <span>Target</span>
                            <span class="metric-val">{{ fmt(Target) }}</span>
                        </div>
                        <div class="metric-row">
                            <span>USL</span>
                            <span class="metric-val">{{ fmt(USL) }}</span>
                        </div>
                        <div class="metric-row">
                            <span>Sample Mean</span>
                            <span class="metric-val">{{ fmt(sampleMean, 4) }}</span>
                        </div>
                        <div class="metric-row">
                            <span>Sample N</span>
                            <span class="metric-val">{{ allValues.length }}</span>
                        </div>
                        <div class="metric-row">
                            <span>StDev (Within)</span>
                            <span class="metric-val">{{ fmt(stdevWithin, 6) }}</span>
                        </div>
                        <div class="metric-row">
                            <span>StDev (Overall)</span>
                            <span class="metric-val">{{ fmt(stdevOverall, 6) }}</span>
                        </div>
                    </div>

                    <div class="right-divider"></div>

                    <!-- Capability -->
                    <div class="capability-grid">
                        <div class="cap-col">
                            <div class="metrics-group-label">POTENTIAL (WITHIN) CAPABILITY</div>
                            <div class="metric-row">
                                <span>Cp</span>
                                <span class="metric-val">{{ fmt(Cp) }}</span>
                            </div>
                            <div class="metric-row">
                                <span>Cpl</span>
                                <span class="metric-val">{{ fmt(Cpl) }}</span>
                            </div>
                            <div class="metric-row">
                                <span>Cpu</span>
                                <span class="metric-val">{{ fmt(Cpu) }}</span>
                            </div>
                            <div class="metric-row">
                                <span>Cpk</span>
                                <span class="metric-val">{{ fmt(Cpk) }}</span>
                            </div>
                            <div class="metric-row">
                                <span class="red-label">CCpk</span>
                                <span class="metric-val red-label">{{ fmt(CCpk) }}</span>
                            </div>
                        </div>
                        <div class="cap-col">
                            <div class="metrics-group-label">OVERALL CAPABILITY</div>
                            <div class="metric-row">
                                <span>Pp</span>
                                <span class="metric-val">{{ fmt(Pp) }}</span>
                            </div>
                            <div class="metric-row">
                                <span>Ppl</span>
                                <span class="metric-val">{{ fmt(Ppl) }}</span>
                            </div>
                            <div class="metric-row">
                                <span>Ppu</span>
                                <span class="metric-val">{{ fmt(Ppu) }}</span>
                            </div>
                            <div class="metric-row">
                                <span>Ppk</span>
                                <span class="metric-val">{{ fmt(Ppk) }}</span>
                            </div>
                            <div class="metric-row">
                                <span class="red-label">Cpm</span>
                                <span class="metric-val red-label">{{ fmt(Cpm) }}</span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Fallback if no data -->
        <div v-else class="monitor-wrapper" style="justify-content:center;align-items:center">
            <p style="color:#888">No data available</p>
        </div>
    </v-dialog>
</template>

<!-- ====================================== -->
<!-- STYLES -->
<!-- ====================================== -->
<style scoped>
/* ===== WRAPPER ===== */
.monitor-wrapper {
    display: flex;
    flex-direction: column;
    height: 100vh;
    background: #f0f4f8;
    font-family: 'Segoe UI', system-ui, sans-serif;
    overflow: hidden;
}

/* ===== TITLE BAR ===== */
.monitor-titlebar {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 10px 20px;
    background: #fff;
    border-bottom: 2px solid #e0e0e0;
    flex-shrink: 0;
}

.monitor-title {
    font-size: 20px;
    font-weight: 700;
    color: #1a2a3a;
    letter-spacing: 0.3px;
}

.titlebar-right {
    display: flex;
    align-items: center;
    gap: 14px;
}

.ready-chip {
    display: flex;
    align-items: center;
    gap: 6px;
    background: #e8f5e9;
    border: 1.5px solid #4caf50;
    border-radius: 20px;
    padding: 4px 12px;
    font-size: 12px;
    font-weight: 700;
    color: #2e7d32;
    letter-spacing: 0.5px;
}

.ready-dot {
    width: 8px;
    height: 8px;
    border-radius: 50%;
    background: #4caf50;
    box-shadow: 0 0 6px #4caf50;
}

.monitor-clock {
    font-size: 16px;
    font-weight: 600;
    color: #333;
    font-variant-numeric: tabular-nums;
    min-width: 72px;
}

.close-btn {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 32px;
    height: 32px;
    border-radius: 8px;
    border: 1px solid #ddd;
    background: #f5f5f5;
    cursor: pointer;
    color: #555;
    transition: all 0.15s;
}

.close-btn:hover {
    background: #fee2e2;
    border-color: #f87171;
    color: #dc2626;
}

/* ===== BODY ===== */
.monitor-body {
    display: flex;
    flex: 1;
    overflow: hidden;
    gap: 0;
}

/* ===== LEFT PANEL ===== */
.left-panel {
    width: 220px;
    flex-shrink: 0;
    background: #fff;
    border-right: 1px solid #e0e0e0;
    padding: 14px 12px;
    display: flex;
    flex-direction: column;
    gap: 8px;
    overflow-y: auto;
}

.field-group {
    display: flex;
    flex-direction: column;
    gap: 2px;
}

.field-label {
    font-size: 10px;
    font-weight: 600;
    color: #888;
    text-transform: uppercase;
    letter-spacing: 0.5px;
}

.field-input {
    background: #f5f7fa;
    border: 1px solid #d9e0ea;
    border-radius: 6px;
    padding: 6px 10px;
    font-size: 13px;
    color: #222;
    font-weight: 500;
}

.field-small {
    flex: 1;
    min-width: 0;
    text-align: center;
    font-size: 12px;
}

.pol-lot-row {
    display: flex;
    align-items: center;
    gap: 4px;
}

.lot-sep {
    color: #888;
    font-weight: 700;
}

.connect-btn {
    background: #1976d2;
    color: white;
    border: none;
    border-radius: 8px;
    padding: 8px;
    font-size: 13px;
    font-weight: 600;
    cursor: pointer;
    transition: background 0.15s;
    letter-spacing: 0.3px;
}

.connect-btn:hover {
    background: #1565c0;
}

.result-badge {
    margin-top: auto;
    border-radius: 10px;
    padding: 12px 16px;
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 2px;
}

.result-label {
    font-size: 10px;
    font-weight: 700;
    color: rgba(255, 255, 255, 0.8);
    letter-spacing: 1px;
    text-transform: uppercase;
}

.result-value {
    font-size: 18px;
    font-weight: 800;
    color: white;
    letter-spacing: 0.5px;
}

/* ===== CENTER PANEL ===== */
.center-panel {
    flex: 1;
    display: flex;
    flex-direction: column;
    overflow: hidden;
    padding: 10px;
    gap: 10px;
    min-width: 0;
}

/* ===== MONITOR SECTION ===== */
.monitor-section {
    background: #fff;
    border-radius: 10px;
    border: 1px solid #d0daea;
    overflow: hidden;
    flex-shrink: 0;
}

.section-header {
    background: #1e5799;
    color: white;
    padding: 8px 14px;
    font-size: 13px;
    font-weight: 700;
    letter-spacing: 0.5px;
}

.grid-row {
    display: flex;
    align-items: center;
    padding: 8px 12px;
    gap: 10px;
}

.row-label-badge {
    width: 38px;
    flex-shrink: 0;
    border-radius: 6px;
    padding: 6px 4px;
    text-align: center;
    font-size: 12px;
    font-weight: 800;
    letter-spacing: 0.5px;
}

.in-badge {
    background: #1e3a5f;
    color: white;
}

.out-badge {
    background: #333;
    color: white;
}

.cassette-block {
    flex: 1;
    display: flex;
    flex-direction: column;
    gap: 3px;
    background: #e8f0fb;
    border-radius: 8px;
    padding: 6px 8px;
    border: 1px solid #c5d5ea;
}

.wafer-row {
    display: flex;
    gap: 4px;
    justify-content: space-around;
}

.wafer-row.single-cell {
    justify-content: center;
}

.wafer-cell {
    flex: 0 0 auto;
    min-width: 46px;
    border-radius: 5px;
    padding: 4px 5px;
    font-size: 11px;
    font-weight: 700;
    text-align: center;
    line-height: 1.2;
}

/* ===== CHARTS ROW ===== */
.charts-row {
    display: flex;
    flex: 1;
    gap: 10px;
    min-height: 0;
}

.chart-box {
    flex: 1;
    background: #fff;
    border-radius: 10px;
    border: 1px solid #d0daea;
    display: flex;
    flex-direction: column;
    overflow: hidden;
}

.chart-box-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    background: #1e5799;
    color: white;
    padding: 8px 14px;
    font-size: 12px;
    font-weight: 700;
    letter-spacing: 0.5px;
    flex-shrink: 0;
}

.chart-unit {
    font-size: 11px;
    font-weight: 500;
    opacity: 0.85;
}

.spec-info {
    font-size: 10px;
    font-weight: 500;
    opacity: 0.9;
}

.chart-area {
    flex: 1;
    padding: 8px;
    min-height: 0;
    position: relative;
}

/* ===== RIGHT PANEL ===== */
.right-panel {
    width: 230px;
    flex-shrink: 0;
    background: #fff;
    border-left: 1px solid #e0e0e0;
    padding: 14px 12px;
    overflow-y: auto;
    display: flex;
    flex-direction: column;
    gap: 6px;
}

.right-section-title {
    font-size: 13px;
    font-weight: 700;
    color: #1a2a3a;
    border-bottom: 2px solid #1e5799;
    padding-bottom: 6px;
    margin-bottom: 4px;
}

.metrics-group {
    display: flex;
    flex-direction: column;
    gap: 3px;
}

.metrics-group-label {
    font-size: 9px;
    font-weight: 700;
    color: #888;
    letter-spacing: 0.7px;
    text-transform: uppercase;
    margin-bottom: 2px;
}

.metric-row {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 2px 0;
    font-size: 12px;
    color: #333;
}

.metric-val {
    font-weight: 700;
    color: #1a2a3a;
    font-variant-numeric: tabular-nums;
}

.diff-val {
    color: #e53935 !important;
}

.red-label {
    color: #e53935 !important;
}

.right-divider {
    height: 1px;
    background: #e0e0e0;
    margin: 4px 0;
}

/* ===== CAPABILITY GRID ===== */
.capability-grid {
    display: flex;
    gap: 8px;
}

.cap-col {
    flex: 1;
    display: flex;
    flex-direction: column;
    gap: 3px;
}
</style>
