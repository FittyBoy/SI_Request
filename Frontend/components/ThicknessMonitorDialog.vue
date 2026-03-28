<script setup lang="ts">
// ====================================== 
// THICKNESS MEASUREMENT MONITOR DIALOG
// optimised: deferred chart render + O(n) histogram + cached cell styles
// ====================================== 
import { computed, ref, shallowRef, watch, onMounted, onUnmounted } from 'vue'
import { Bar } from 'vue-chartjs'
import {
    Chart as ChartJS,
    BarController, CategoryScale, LinearScale, BarElement,
    LineController, LineElement, PointElement,
    Filler, Tooltip, Legend,
} from 'chart.js'
import annotationPlugin from 'chartjs-plugin-annotation'
import { X } from 'lucide-vue-next'

ChartJS.register(
    BarController, CategoryScale, LinearScale, BarElement,
    LineController, LineElement, PointElement,
    Filler, Tooltip, Legend, annotationPlugin
)

// ===== PROPS & EMITS =====
interface Threshold { Value: number; Label: string; Color: string }
const props = defineProps<{
    modelValue: boolean
    lotData: any | null
    thresholds: Threshold[]
}>()
const emit = defineEmits(['update:modelValue'])
const close = () => emit('update:modelValue', false)

// ===== PERFORMANCE: deferred chart rendering =====
// Charts render AFTER the dialog slide-in animation completes (~300 ms)
// so the opening feels instant and the animation is smooth
const chartsReady = ref(false)
let chartsReadyTimer: ReturnType<typeof setTimeout> | null = null

const onDialogAfterEnter = () => {
    chartsReady.value = true
    if (chartsReadyTimer) { clearTimeout(chartsReadyTimer); chartsReadyTimer = null }
}

watch(() => props.modelValue, (open) => {
    if (open) {
        // fallback: ถ้า after-enter ไม่ยิงภายใน 600ms ให้ render chart เลย
        chartsReadyTimer = setTimeout(() => {
            if (!chartsReady.value) chartsReady.value = true
        }, 600)
    } else {
        chartsReady.value = false
        if (chartsReadyTimer) { clearTimeout(chartsReadyTimer); chartsReadyTimer = null }
    }
})

// ===== LIVE CLOCK =====
const currentTime = ref('')
let clockTimer: ReturnType<typeof setInterval> | null = null
const updateClock = () => {
    const now = new Date()
    currentTime.value = [now.getHours(), now.getMinutes(), now.getSeconds()]
        .map(n => String(n).padStart(2, '0')).join(':')
}
onMounted(() => { updateClock(); clockTimer = setInterval(updateClock, 1000) })
onUnmounted(() => {
    if (clockTimer) clearInterval(clockTimer)
    if (chartsReadyTimer) clearTimeout(chartsReadyTimer)
})

// ===== THRESHOLD HELPERS (computed once, reused everywhere) =====
const sortedThresholds = computed(() =>
    [...props.thresholds].sort((a, b) => b.Value - a.Value)
)
const getThresholdByLabel = (label: string) =>
    props.thresholds.find(t => t.Label === label)?.Value ?? 0

const USL        = computed(() => getThresholdByLabel('Hold'))
const Target     = computed(() => getThresholdByLabel('Targetbar'))
const LSL        = computed(() => Target.value - (USL.value - Target.value))
const RescreenUpper = computed(() => {
    const rs = props.thresholds.filter(t => t.Label === 'Re-screen')
    return rs.length ? Math.max(...rs.map(t => t.Value)) : USL.value
})
const RescreenLower = computed(() => {
    const rs = props.thresholds.filter(t => t.Label === 'Re-screen')
    return rs.length > 1 ? Math.min(...rs.map(t => t.Value)) : Target.value - (USL.value - Target.value)
})

// ===== CELL COLOUR: cached per (threshold set) — not re-sorted every call =====
const BG_MAP: Record<string, string>     = { blue:'#bbdefb', orange:'#ffe0b2', green:'#c8e6c9', red:'#ffcdd2' }
const BORDER_MAP: Record<string, string> = { blue:'#1976d2', orange:'#f57c00', green:'#388e3c', red:'#d32f2f' }

const getCellStyle = (value: number | null) => {
    if (value == null) return { background:'#e0e0e0', color:'#888', border:'1.5px solid #bbb' }
    const ts = sortedThresholds.value
    for (const t of ts) {
        if (value >= t.Value) return {
            background : BG_MAP[t.Color]     || '#e0e0e0',
            color      : BORDER_MAP[t.Color] || '#333',
            border     : `1.5px solid ${BORDER_MAP[t.Color] || '#aaa'}`,
        }
    }
    const last = ts[ts.length - 1]
    return {
        background : BG_MAP[last?.Color]     || '#ffcdd2',
        color      : BORDER_MAP[last?.Color] || '#d32f2f',
        border     : `1.5px solid ${BORDER_MAP[last?.Color] || '#d32f2f'}`,
    }
}

// ===== MEASUREMENT ARRAYS =====
const carriers = ['Ca1','Ca2','Ca3','Ca4','Ca5']
const getArr   = (key: string): (number|null)[] => props.lotData?.[key] || []

const caInArrays  = computed(() => carriers.map(ca => getArr(ca + 'In')))
const caOutArrays = computed(() => carriers.map(ca => getArr(ca + 'Out')))

// flat numeric arrays
const allInValues  = computed(() => caInArrays.value.flat().filter((v): v is number => v != null))
const allOutValues = computed(() => caOutArrays.value.flat().filter((v): v is number => v != null))
const allValues    = computed(() => [...allInValues.value, ...allOutValues.value])

// 5-point wafer layout
const layout5 = [[0,1],[2],[3,4]]
const getPointValue = (arr: (number|null)[], idx: number) => arr?.[idx] ?? null

// ===== STATISTICS =====
const _mean = (arr: number[]) => arr.length ? arr.reduce((a,b)=>a+b,0)/arr.length : 0
const _stdev = (arr: number[]) => {
    if (arr.length < 2) return 0
    const m = _mean(arr)
    return Math.sqrt(arr.reduce((s,v)=>s+(v-m)**2,0)/(arr.length-1))
}

const sampleMean    = computed(() => _mean(allValues.value))
const stdevOverall  = computed(() => _stdev(allValues.value))

// Within StDev: RMS of sub-group stdevs
const stdevWithin = computed(() => {
    const subs: number[] = []
    carriers.forEach((_,i) => {
        const iv = caInArrays.value[i].filter((v): v is number => v != null)
        const ov = caOutArrays.value[i].filter((v): v is number => v != null)
        if (iv.length > 1) subs.push(_stdev(iv))
        if (ov.length > 1) subs.push(_stdev(ov))
    })
    if (!subs.length) return stdevOverall.value
    return Math.sqrt(subs.reduce((s,v)=>s+v*v,0)/subs.length)
})

const Cp  = computed(() => stdevWithin.value  > 0 ? (USL.value-LSL.value)/(6*stdevWithin.value)  : 0)
const Cpl = computed(() => stdevWithin.value  > 0 ? (sampleMean.value-LSL.value)/(3*stdevWithin.value)  : 0)
const Cpu = computed(() => stdevWithin.value  > 0 ? (USL.value-sampleMean.value)/(3*stdevWithin.value)  : 0)
const Cpk = computed(() => Math.min(Cpl.value, Cpu.value))
const Pp  = computed(() => stdevOverall.value > 0 ? (USL.value-LSL.value)/(6*stdevOverall.value) : 0)
const Ppl = computed(() => stdevOverall.value > 0 ? (sampleMean.value-LSL.value)/(3*stdevOverall.value) : 0)
const Ppu = computed(() => stdevOverall.value > 0 ? (USL.value-sampleMean.value)/(3*stdevOverall.value) : 0)
const Ppk = computed(() => Math.min(Ppl.value, Ppu.value))
const CCpk = computed(() => Cpk.value)
const Cpm  = computed(() => {
    if (!stdevWithin.value) return 0
    const t = (sampleMean.value - Target.value) / stdevWithin.value
    return Cp.value / Math.sqrt(1 + t*t)
})

const fmt = (v: number, d=3) => (isNaN(v)||!isFinite(v)) ? 'N/A' : v.toFixed(d)

// Derived metrics
const maxValue      = computed(() => allValues.value.length ? Math.max(...allValues.value) : 0)
const minValue      = computed(() => allValues.value.length ? Math.min(...allValues.value) : 0)
const differenceNm  = computed(() => Math.round((maxValue.value - minValue.value)*1000))
const removalNm     = computed(() => {
    const tb = props.lotData?.ThBefore ?? props.lotData?.thBefore
    return (tb && sampleMean.value) ? Math.round((tb - sampleMean.value)*1000)
           : (props.lotData?.Margin ?? props.lotData?.margin ?? '-')
})
const rate = computed(() => props.lotData?.Rate ?? props.lotData?.poRate ?? props.lotData?.PoRate ?? '-')

// Lot info
const lotNumber    = computed(() => props.lotData?.LotId ?? props.lotData?.imobileLot ?? '-')
const polLotParts  = computed(() => ({
    lp: props.lotData?.LotPo ?? props.lotData?.lotPo ?? '',
    mc: props.lotData?.McPo  ?? props.lotData?.mcPo  ?? '',
    no: props.lotData?.NoPo  ?? props.lotData?.noPo  ?? '',
}))
const productSize   = computed(() => props.lotData?.Size ?? props.lotData?.imobileSize ?? '-')
const process       = computed(() => props.lotData?.Process ?? props.lotData?.process  ?? '-')
const mc            = computed(() => props.lotData?.McPo ?? props.lotData?.mcPo ?? '-')
const thBefore      = computed(() => { const v = props.lotData?.ThBefore ?? props.lotData?.thBefore; return v!=null?Number(v).toFixed(3):'-' })
const processingTime= computed(() => props.lotData?.ProcessTime ?? props.lotData?.processTime ?? '-')
const cassetteNo    = computed(() => {
    const lp = props.lotData?.LotPo ?? props.lotData?.lotPo ?? ''
    const m  = props.lotData?.McPo  ?? props.lotData?.mcPo  ?? ''
    return lp&&m ? `${lp}-${m}` : '-'
})
const programNo     = computed(() => props.lotData?.Program ?? props.lotData?.program ?? '0')
const result        = computed(() => props.lotData?.Result ?? props.lotData?.result ?? props.lotData?.Status ?? '-')
const resultColor   = computed(() => {
    const r = result.value?.toLowerCase()??''
    if (r==='ok')    return '#2e7d32'
    if (r==='hold')  return '#1565c0'
    if (r==='scrap') return '#c62828'
    if (r.includes('rescreen')||r.includes('re-screen')) return '#e65100'
    return '#555'
})

// ===== HISTOGRAM — O(n) bucket pass =====
const HIST_STEP = 0.002
const histBuckets = computed(() => {
    const vals = allValues.value
    if (!vals.length) return { labels:[], counts:[], centers:[] }

    const lo = Math.min(LSL.value-0.002, Math.min(...vals)-0.002)
    const hi = Math.max(USL.value+0.002, Math.max(...vals)+0.002)
    const n  = Math.ceil((hi - lo) / HIST_STEP)

    const counts  = new Array(n).fill(0)
    const labels  : string[] = []
    const centers : number[] = []

    for (let i=0; i<n; i++) {
        const edge = lo + i*HIST_STEP
        labels.push(edge.toFixed(3))
        centers.push(edge + HIST_STEP/2)
    }
    // single O(n) pass
    for (const v of vals) {
        const idx = Math.floor((v - lo) / HIST_STEP)
        if (idx >= 0 && idx < n) counts[idx]++
    }
    return { labels, counts, centers }
})

const normalPDF = (x: number, mu: number, sigma: number) =>
    sigma===0 ? 0 : Math.exp(-0.5*((x-mu)/sigma)**2) / (sigma*Math.sqrt(2*Math.PI))

// shallowRef avoids deep reactivity overhead for large chart data objects
const cpkChartData = shallowRef<any>({ labels:[], datasets:[] })
const trendChartData = shallowRef<any>({ labels:[], datasets:[] })

// Rebuild chart data only when chartsReady flips to true OR source data changes
// Using watch instead of computed avoids re-running when dialog is closed
watch([chartsReady, allValues, histBuckets, sampleMean, stdevWithin, stdevOverall, LSL, Target, USL, RescreenUpper, RescreenLower], () => {
    if (!chartsReady.value) return

    // ── CPK chart ──
    const { labels, counts, centers } = histBuckets.value
    if (labels.length) {
        const maxCount = Math.max(...counts, 1)
        const n = allValues.value.length
        const peakWithin  = normalPDF(sampleMean.value, sampleMean.value, stdevWithin.value)  || 1
        const peakOverall = normalPDF(sampleMean.value, sampleMean.value, stdevOverall.value) || 1
        const scale = (peak: number) => maxCount / (n * HIST_STEP * peak)

        cpkChartData.value = {
            labels,
            datasets: [
                { type:'bar',  label:'Frequency',   data: counts,
                  backgroundColor:'rgba(33,150,243,0.65)', borderColor:'#1565c0', borderWidth:1, order:2 },
                { type:'line', label:'Within Fit',  data: centers.map(c => normalPDF(c,sampleMean.value,stdevWithin.value)*n*HIST_STEP*scale(peakWithin)),
                  borderColor:'#111', borderWidth:2, borderDash:[5,3], pointRadius:0, fill:false, order:1 },
                { type:'line', label:'Overall Fit', data: centers.map(c => normalPDF(c,sampleMean.value,stdevOverall.value)*n*HIST_STEP*scale(peakOverall)),
                  borderColor:'#e53935', borderWidth:2, borderDash:[5,3], pointRadius:0, fill:false, order:0 },
            ],
        }
    }

    // ── Trend chart ──
    const allPoints: {x:number,y:number}[] = []
    carriers.forEach((_,i) => {
        caInArrays.value[i].forEach(v  => { if(v!=null) allPoints.push({x:i*2,  y:v}) })
        caOutArrays.value[i].forEach(v => { if(v!=null) allPoints.push({x:i*2+1,y:v}) })
    })
    const avgByX = Array.from({length:10},(_,xi) => {
        const pts = allPoints.filter(p=>p.x===xi).map(p=>p.y)
        return pts.length ? {x:xi, y:_mean(pts)} : null
    }).filter(Boolean)

    trendChartData.value = {
        labels: ['C1 IN','C1 OUT','C2 IN','C2 OUT','C3 IN','C3 OUT','C4 IN','C4 OUT','C5 IN','C5 OUT'],
        datasets: [
            { type:'scatter', label:'Measurements', data:allPoints, backgroundColor:'#2196f3', pointRadius:5, order:2 },
            { type:'line', label:'Trend', data:avgByX, borderColor:'#1565c0', borderWidth:1.5, pointRadius:0, fill:false, tension:0, order:1 },
        ],
    }
}, { immediate: false })

// ===== CHART OPTIONS (static objects — not computed) =====
const TREND_AXIS_LABELS = ['C1 IN','C1 OUT','C2 IN','C2 OUT','C3 IN','C3 OUT','C4 IN','C4 OUT','C5 IN','C5 OUT']

const trendChartOptions = computed(() => {
    const vals = allValues.value
    const yMin = vals.length ? Math.min(...vals, LSL.value)-0.003 : 0.180
    const yMax = vals.length ? Math.max(...vals, USL.value)+0.003 : 0.220
    return {
        responsive:true, maintainAspectRatio:false,
        animation: false,        // ← no per-point animation = instant render
        plugins:{
            legend:{ display:false },
            annotation:{ annotations:{
                holdLine:       { type:'line', yMin:USL.value,         yMax:USL.value,         borderColor:'#1565c0', borderWidth:1.5, label:{content:'Hold',  display:true,position:'end',font:{size:9},color:'#1565c0'} },
                rescreenLine:   { type:'line', yMin:RescreenUpper.value,yMax:RescreenUpper.value,borderColor:'#f57c00', borderWidth:1.5, label:{content:'Re-scr',display:true,position:'end',font:{size:9},color:'#f57c00'} },
                targetLine:     { type:'line', yMin:Target.value,      yMax:Target.value,      borderColor:'#388e3c', borderWidth:1.5, borderDash:[4,3], label:{content:'Target',display:true,position:'end',font:{size:9},color:'#388e3c'} },
                rescreenLowLine:{ type:'line', yMin:RescreenLower.value,yMax:RescreenLower.value,borderColor:'#f57c00', borderWidth:1.5 },
                lslLine:        { type:'line', yMin:LSL.value,         yMax:LSL.value,         borderColor:'#d32f2f', borderWidth:1.5 },
            }},
            tooltip:{ callbacks:{ label:(ctx:any)=>`${Number(ctx.parsed.y).toFixed(3)} μm` }},
        },
        scales:{
            x:{ type:'linear' as const, min:-0.5, max:9.5,
                ticks:{ stepSize:1, font:{size:9}, maxRotation:45, callback:(_:any,i:number)=>TREND_AXIS_LABELS[i]||'' },
                grid:{ color:'rgba(0,0,0,0.07)' } },
            y:{ min:yMin, max:yMax,
                ticks:{ font:{size:9}, callback:(v:any)=>Number(v).toFixed(3), stepSize:0.005 },
                grid:{ color:'rgba(0,0,0,0.07)' } },
        },
    }
})

const cpkChartOptions = computed(() => {
    const { labels } = histBuckets.value
    const toIdx = (val: number) => labels.length ? (val - parseFloat(labels[0])) / HIST_STEP : 0
    return {
        responsive:true, maintainAspectRatio:false,
        animation: false,
        plugins:{
            legend:{ display:true, position:'top' as const, labels:{ font:{size:10}, boxWidth:20 } },
            annotation:{ annotations: labels.length ? {
                lslLine:   { type:'line', xMin:toIdx(LSL.value),    xMax:toIdx(LSL.value),    borderColor:'#e53935', borderWidth:2, borderDash:[6,3] },
                targetLine:{ type:'line', xMin:toIdx(Target.value), xMax:toIdx(Target.value), borderColor:'#388e3c', borderWidth:2, borderDash:[6,3] },
                uslLine:   { type:'line', xMin:toIdx(USL.value),    xMax:toIdx(USL.value),    borderColor:'#1565c0', borderWidth:2, borderDash:[6,3] },
            } : {} },
            tooltip:{ enabled:false },
        },
        scales:{
            x:{ ticks:{ font:{size:9}, maxRotation:45, callback:(_:any,i:number)=>i%2===0?(histBuckets.value.labels[i]||''):'' }, grid:{ display:false } },
            y:{ beginAtZero:true, ticks:{ font:{size:9}, stepSize:1 }, grid:{ color:'rgba(0,0,0,0.07)' } },
        },
    }
})
</script>

<!-- ====================================== -->
<!-- TEMPLATE -->
<!-- ====================================== -->
<template>
    <!-- no transition → snappy open; after-enter fires chart render -->
    <v-dialog
        :model-value="modelValue"
        @update:model-value="emit('update:modelValue', $event)"
        @after-enter="onDialogAfterEnter"
        fullscreen
        transition="dialog-bottom-transition"
        scrollable
    >
        <div class="monitor-wrapper" v-if="lotData">

            <!-- ===== TITLE BAR ===== -->
            <div class="monitor-titlebar">
                <span class="monitor-title">Thickness Measurement Monitor</span>
                <div class="titlebar-right">
                    <div class="ready-chip"><span class="ready-dot"></span>READY</div>
                    <span class="monitor-clock">{{ currentTime }}</span>
                    <button class="close-btn" @click="close"><X :size="18" /></button>
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
                    <div class="field-group"><label class="field-label">Product Size</label><div class="field-input">{{ productSize }}</div></div>
                    <div class="field-group"><label class="field-label">Process</label><div class="field-input">{{ process }}</div></div>
                    <div class="field-group"><label class="field-label">MC</label><div class="field-input">{{ mc }}</div></div>
                    <div class="field-group"><label class="field-label">Thickness before</label><div class="field-input">{{ thBefore }}</div></div>
                    <div class="field-group"><label class="field-label">Processing Time (min)</label><div class="field-input">{{ processingTime }}</div></div>
                    <div class="field-group"><label class="field-label">Cassette no</label><div class="field-input">{{ cassetteNo }}</div></div>
                    <div class="field-group"><label class="field-label">Program no</label><div class="field-input">{{ programNo }}</div></div>
                    <div class="result-badge" :style="{ background: resultColor }">
                        <span class="result-label">RESULT</span>
                        <span class="result-value">{{ result }}</span>
                    </div>
                </div>

                <!-- CENTER PANEL -->
                <div class="center-panel">

                    <!-- MONITOR GRID — renders immediately (no charts here) -->
                    <div class="monitor-section">
                        <div class="section-header">Monitor</div>
                        <div class="grid-row">
                            <div class="row-label-badge in-badge">IN</div>
                            <div v-for="(caIn, ci) in caInArrays" :key="`in-${ci}`" class="cassette-block">
                                <template v-for="(row, ri) in layout5" :key="`r${ri}`">
                                    <div class="wafer-row" :class="{ 'single-cell': row.length === 1 }">
                                        <div v-for="idx in row" :key="idx" class="wafer-cell"
                                            :style="getCellStyle(getPointValue(caIn, idx))">
                                            {{ getPointValue(caIn, idx) != null ? Number(getPointValue(caIn, idx)).toFixed(3) : '-' }}
                                        </div>
                                    </div>
                                </template>
                            </div>
                        </div>
                        <div class="grid-row" style="margin-top:4px">
                            <div class="row-label-badge out-badge">OUT</div>
                            <div v-for="(caOut, ci) in caOutArrays" :key="`out-${ci}`" class="cassette-block">
                                <template v-for="(row, ri) in layout5" :key="`r${ri}`">
                                    <div class="wafer-row" :class="{ 'single-cell': row.length === 1 }">
                                        <div v-for="idx in row" :key="idx" class="wafer-cell"
                                            :style="getCellStyle(getPointValue(caOut, idx))">
                                            {{ getPointValue(caOut, idx) != null ? Number(getPointValue(caOut, idx)).toFixed(3) : '-' }}
                                        </div>
                                    </div>
                                </template>
                            </div>
                        </div>
                    </div>

                    <!-- CHARTS ROW — appear after transition -->
                    <div class="charts-row">
                        <!-- Thickness Trend -->
                        <div class="chart-box">
                            <div class="chart-box-header">
                                <span>Thickness Trend</span>
                                <span class="chart-unit">μm</span>
                            </div>
                            <div class="chart-area">
                                <div v-if="!chartsReady" class="chart-skeleton"></div>
                                <Bar v-else :data="trendChartData" :options="trendChartOptions as any" />
                            </div>
                        </div>

                        <!-- Process Capability -->
                        <div class="chart-box">
                            <div class="chart-box-header">
                                <span>Process Capability (Cpk)</span>
                                <span class="spec-info">
                                    LSL: {{ LSL.toFixed(3) }} | Target: {{ Target.toFixed(3) }} | USL: {{ USL.toFixed(3) }}
                                </span>
                            </div>
                            <div class="chart-area">
                                <div v-if="!chartsReady" class="chart-skeleton"></div>
                                <Bar v-else :data="cpkChartData" :options="cpkChartOptions as any" />
                            </div>
                        </div>
                    </div>
                </div>

                <!-- RIGHT PANEL -->
                <div class="right-panel">
                    <div class="right-section-title">Averages &amp; Metrics</div>
                    <div class="metrics-group">
                        <div class="metrics-group-label">AVERAGES</div>
                        <div class="metric-row"><span>Average</span><span class="metric-val">{{ fmt(sampleMean) }}</span></div>
                        <div class="metric-row"><span>Rate</span><span class="metric-val">{{ typeof rate==='number'?fmt(rate):rate }}</span></div>
                        <div class="metric-row"><span>Difference</span><span class="metric-val diff-val">{{ differenceNm }}</span></div>
                        <div class="metric-row"><span>Removal</span><span class="metric-val">{{ removalNm }}</span></div>
                    </div>
                    <div class="metrics-group">
                        <div class="metrics-group-label">EXTREMES</div>
                        <div class="metric-row"><span>Max Value</span><span class="metric-val">{{ fmt(maxValue) }}</span></div>
                        <div class="metric-row"><span>Min Value</span><span class="metric-val">{{ fmt(minValue) }}</span></div>
                    </div>
                    <div class="right-divider"></div>
                    <div class="metrics-group">
                        <div class="metrics-group-label">PROCESS DATA</div>
                        <div class="metric-row"><span>LSL</span><span class="metric-val">{{ fmt(LSL) }}</span></div>
                        <div class="metric-row"><span>Target</span><span class="metric-val">{{ fmt(Target) }}</span></div>
                        <div class="metric-row"><span>USL</span><span class="metric-val">{{ fmt(USL) }}</span></div>
                        <div class="metric-row"><span>Sample Mean</span><span class="metric-val">{{ fmt(sampleMean,4) }}</span></div>
                        <div class="metric-row"><span>Sample N</span><span class="metric-val">{{ allValues.length }}</span></div>
                        <div class="metric-row"><span>StDev (Within)</span><span class="metric-val">{{ fmt(stdevWithin,6) }}</span></div>
                        <div class="metric-row"><span>StDev (Overall)</span><span class="metric-val">{{ fmt(stdevOverall,6) }}</span></div>
                    </div>
                    <div class="right-divider"></div>
                    <div class="capability-grid">
                        <div class="cap-col">
                            <div class="metrics-group-label">POTENTIAL (WITHIN)</div>
                            <div class="metric-row"><span>Cp</span><span class="metric-val">{{ fmt(Cp) }}</span></div>
                            <div class="metric-row"><span>Cpl</span><span class="metric-val">{{ fmt(Cpl) }}</span></div>
                            <div class="metric-row"><span>Cpu</span><span class="metric-val">{{ fmt(Cpu) }}</span></div>
                            <div class="metric-row"><span>Cpk</span><span class="metric-val">{{ fmt(Cpk) }}</span></div>
                            <div class="metric-row"><span class="red-label">CCpk</span><span class="metric-val red-label">{{ fmt(CCpk) }}</span></div>
                        </div>
                        <div class="cap-col">
                            <div class="metrics-group-label">OVERALL</div>
                            <div class="metric-row"><span>Pp</span><span class="metric-val">{{ fmt(Pp) }}</span></div>
                            <div class="metric-row"><span>Ppl</span><span class="metric-val">{{ fmt(Ppl) }}</span></div>
                            <div class="metric-row"><span>Ppu</span><span class="metric-val">{{ fmt(Ppu) }}</span></div>
                            <div class="metric-row"><span>Ppk</span><span class="metric-val">{{ fmt(Ppk) }}</span></div>
                            <div class="metric-row"><span class="red-label">Cpm</span><span class="metric-val red-label">{{ fmt(Cpm) }}</span></div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div v-else class="monitor-wrapper" style="justify-content:center;align-items:center">
            <p style="color:#888">No data available</p>
        </div>
    </v-dialog>
</template>

<style scoped>
.monitor-wrapper { display:flex; flex-direction:column; height:100vh; background:#f0f4f8; font-family:'Segoe UI',system-ui,sans-serif; overflow:hidden; }

/* Title bar */
.monitor-titlebar { display:flex; align-items:center; justify-content:space-between; padding:10px 20px; background:#fff; border-bottom:2px solid #e0e0e0; flex-shrink:0; }
.monitor-title { font-size:20px; font-weight:700; color:#1a2a3a; }
.titlebar-right { display:flex; align-items:center; gap:14px; }
.ready-chip { display:flex; align-items:center; gap:6px; background:#e8f5e9; border:1.5px solid #4caf50; border-radius:20px; padding:4px 12px; font-size:12px; font-weight:700; color:#2e7d32; }
.ready-dot { width:8px; height:8px; border-radius:50%; background:#4caf50; box-shadow:0 0 6px #4caf50; }
.monitor-clock { font-size:16px; font-weight:600; color:#333; font-variant-numeric:tabular-nums; min-width:72px; }
.close-btn { display:flex; align-items:center; justify-content:center; width:32px; height:32px; border-radius:8px; border:1px solid #ddd; background:#f5f5f5; cursor:pointer; color:#555; transition:all .15s; }
.close-btn:hover { background:#fee2e2; border-color:#f87171; color:#dc2626; }

/* Body */
.monitor-body { display:flex; flex:1; overflow:hidden; }

/* Left panel */
.left-panel { width:220px; flex-shrink:0; background:#fff; border-right:1px solid #e0e0e0; padding:14px 12px; display:flex; flex-direction:column; gap:8px; overflow-y:auto; }
.field-group { display:flex; flex-direction:column; gap:2px; }
.field-label { font-size:10px; font-weight:600; color:#888; text-transform:uppercase; letter-spacing:.5px; }
.field-input { background:#f5f7fa; border:1px solid #d9e0ea; border-radius:6px; padding:6px 10px; font-size:13px; color:#222; font-weight:500; }
.field-small { flex:1; min-width:0; text-align:center; font-size:12px; }
.pol-lot-row { display:flex; align-items:center; gap:4px; }
.lot-sep { color:#888; font-weight:700; }
.connect-btn { background:#1976d2; color:white; border:none; border-radius:8px; padding:8px; font-size:13px; font-weight:600; cursor:pointer; }
.connect-btn:hover { background:#1565c0; }
.result-badge { margin-top:auto; border-radius:10px; padding:12px 16px; display:flex; flex-direction:column; align-items:center; gap:2px; }
.result-label { font-size:10px; font-weight:700; color:rgba(255,255,255,.8); letter-spacing:1px; text-transform:uppercase; }
.result-value { font-size:18px; font-weight:800; color:white; }

/* Center panel */
.center-panel { flex:1; display:flex; flex-direction:column; overflow:hidden; padding:10px; gap:10px; min-width:0; }

/* Monitor section */
.monitor-section { background:#fff; border-radius:10px; border:1px solid #d0daea; overflow:hidden; flex-shrink:0; }
.section-header { background:#1e5799; color:white; padding:8px 14px; font-size:13px; font-weight:700; letter-spacing:.5px; }
.grid-row { display:flex; align-items:center; padding:8px 12px; gap:10px; }
.row-label-badge { width:38px; flex-shrink:0; border-radius:6px; padding:6px 4px; text-align:center; font-size:12px; font-weight:800; }
.in-badge  { background:#1e3a5f; color:white; }
.out-badge { background:#333; color:white; }
.cassette-block { flex:1; display:flex; flex-direction:column; gap:3px; background:#e8f0fb; border-radius:8px; padding:6px 8px; border:1px solid #c5d5ea; }
.wafer-row { display:flex; gap:4px; justify-content:space-around; }
.wafer-row.single-cell { justify-content:center; }
.wafer-cell { flex:0 0 auto; min-width:46px; border-radius:5px; padding:4px 5px; font-size:11px; font-weight:700; text-align:center; }

/* Charts row */
.charts-row { display:flex; flex:1; gap:10px; min-height:0; }
.chart-box { flex:1; background:#fff; border-radius:10px; border:1px solid #d0daea; display:flex; flex-direction:column; overflow:hidden; }
.chart-box-header { display:flex; justify-content:space-between; align-items:center; background:#1e5799; color:white; padding:8px 14px; font-size:12px; font-weight:700; flex-shrink:0; }
.chart-unit  { font-size:11px; opacity:.85; }
.spec-info   { font-size:10px; opacity:.9; }
.chart-area  { flex:1; padding:8px; min-height:0; position:relative; }

/* Skeleton while charts load */
.chart-skeleton {
    width:100%; height:100%;
    background: linear-gradient(90deg, #f0f4f8 25%, #e2e8f0 50%, #f0f4f8 75%);
    background-size: 200% 100%;
    animation: shimmer 1.2s infinite;
    border-radius: 8px;
}
@keyframes shimmer { 0%{background-position:200% 0} 100%{background-position:-200% 0} }

/* Right panel */
.right-panel { width:230px; flex-shrink:0; background:#fff; border-left:1px solid #e0e0e0; padding:14px 12px; overflow-y:auto; display:flex; flex-direction:column; gap:6px; }
.right-section-title { font-size:13px; font-weight:700; color:#1a2a3a; border-bottom:2px solid #1e5799; padding-bottom:6px; margin-bottom:4px; }
.metrics-group { display:flex; flex-direction:column; gap:3px; }
.metrics-group-label { font-size:9px; font-weight:700; color:#888; letter-spacing:.7px; text-transform:uppercase; margin-bottom:2px; }
.metric-row { display:flex; justify-content:space-between; align-items:center; padding:2px 0; font-size:12px; color:#333; }
.metric-val  { font-weight:700; color:#1a2a3a; font-variant-numeric:tabular-nums; }
.diff-val    { color:#e53935 !important; }
.red-label   { color:#e53935 !important; }
.right-divider { height:1px; background:#e0e0e0; margin:4px 0; }
.capability-grid { display:flex; gap:8px; }
.cap-col { flex:1; display:flex; flex-direction:column; gap:3px; }
</style>
