// ~/types/flowout.ts

export interface LotWithQuantity {
  poLot: string
  imobileLot: string
  mcNo: string
  status: string
  check: 'OK' | 'NG'
  checkSt: boolean
  quantity: number
}

export interface SummaryStats {
  date: string
  mcNo?: string
  totalLots: number
  okCount: number
  ngCount: number
  totalQuantity: number
}

export interface LotsWithQuantityResponse {
  success: boolean
  message?: string
  data: LotWithQuantity[]
  summary: SummaryStats
}

export interface McLotsSummaryResponse {
  success: boolean
  message?: string
  data: LotWithQuantity[]
  summary: {
    mcNo: string
    date: string
    totalCount: number
    okCount: number
    ngCount: number
    totalQuantity: number
  }
}

export interface PoCheckFlowResponse {
  id: string
  poLot: string
  imobileLot: string
  statusTn: string
  checkSt: boolean
  check: 'OK' | 'NG'
  checkDate: string
  mcNo: string
  lotQty: number
}

export interface LotsByMcResponse {
  success: boolean
  message?: string
  data: PoCheckFlowResponse[]
  totalCount: number
  displayCount: number
  okCount: number
  ngCount: number
}

export interface SearchLotResponse {
  success: boolean
  message: string
  data?: {
    imobileLot: string
    poLot: string
    statusTn: string
    checkSt: boolean
    mcNo: string
    hasTH100: boolean
    th100Status: string | null
    isDuplicate: boolean
    existingQty: number | null
  }
}

export interface SaveLotResponse {
  success: boolean
  message: string
  data?: PoCheckFlowResponse
}

export interface McListResponse {
  success: boolean
  message?: string
  data: string[]
}

export interface DailyStatsResponse {
  success: boolean
  message?: string
  data: {
    date: string
    mcNo: string | null
    totalLots: number
    okCount: number
    ngCount: number
    totalQty: number
  }
}