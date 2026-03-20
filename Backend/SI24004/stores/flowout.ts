// stores/flowout.ts
import { defineStore } from 'pinia'
import type { LotRecord, PoCheckFlowResponse } from '~/types/flowout'

interface FlowOutState {
  lotRecords: LotRecord[]
  currentMc: string
  currentDate: string
  summaryData: PoCheckFlowResponse[]
  isLoading: boolean
  error: string | null
  lastAccessDate: string
}

export const useFlowOutStore = defineStore('flowout', {
  state: (): FlowOutState => ({
    lotRecords: [],
    currentMc: '631',
    currentDate: '',
    summaryData: [],
    isLoading: false,
    error: null,
    lastAccessDate: ''
  }),

  getters: {
    /**
     * จำนวน LOT ที่บันทึกแล้ว
     */
    totalLots: (state): number => state.lotRecords.length,

    /**
     * จำนวน LOT ที่มีสถานะ OK
     */
    okCount: (state): number => 
      state.lotRecords.filter(lot => lot.check === 'OK').length,

    /**
     * จำนวน LOT ที่มีสถานะ NG
     */
    ngCount: (state): number => 
      state.lotRecords.filter(lot => lot.check === 'NG').length,

    /**
     * ตรวจสอบว่าสามารถเพิ่ม LOT ได้อีกหรือไม่ (สูงสุด 8 LOT/วัน)
     */
    canAddMore: (state): boolean => state.lotRecords.length < 8,

    /**
     * Summary data จัดกลุ่มตาม Machine
     */
    summaryByMachine: (state): Record<string, PoCheckFlowResponse[]> => {
      return state.summaryData.reduce((acc, item) => {
        const key = `MC${item.mcNo}`
        if (!acc[key]) {
          acc[key] = []
        }
        acc[key].push(item)
        return acc
      }, {} as Record<string, PoCheckFlowResponse[]>)
    },

    /**
     * ตรวจสอบว่าข้ามวันหรือไม่
     */
    isNewDay: (state): boolean => {
      const today = new Date().toISOString().split('T')[0]
      return state.lastAccessDate !== today
    }
  },

  actions: {
    /**
     * เพิ่ม LOT record ใหม่
     */
    addLotRecord(record: Omit<LotRecord, 'lot'>) {
      if (!this.canAddMore) {
        this.error = 'ไม่สามารถเพิ่ม LOT ได้อีก (สูงสุด 8 LOT/วัน)'
        return false
      }

      const newRecord: LotRecord = {
        ...record,
        lot: String(this.lotRecords.length + 1).padStart(2, '0')
      }

      this.lotRecords.push(newRecord)
      this.saveLotRecords()
      return true
    },

    /**
     * ลบ LOT record
     */
    removeLotRecord(index: number) {
      if (index >= 0 && index < this.lotRecords.length) {
        this.lotRecords.splice(index, 1)
        // Re-number remaining lots
        this.lotRecords.forEach((record, idx) => {
          record.lot = String(idx + 1).padStart(2, '0')
        })
        this.saveLotRecords()
      }
    },

    /**
     * ล้างข้อมูล LOT records
     */
    clearLotRecords() {
      this.lotRecords = []
      this.saveLotRecords()
    },

    /**
     * บันทึก LOT records ลง localStorage
     */
    saveLotRecords() {
      if (process.client) {
        localStorage.setItem('lotRecords', JSON.stringify(this.lotRecords))
        localStorage.setItem('lastAccessDate', new Date().toISOString().split('T')[0])
      }
    },

    /**
     * โหลด LOT records จาก localStorage
     */
    loadLotRecords() {
      if (process.client) {
        const savedRecords = localStorage.getItem('lotRecords')
        const savedDate = localStorage.getItem('lastAccessDate')
        const today = new Date().toISOString().split('T')[0]

        this.lastAccessDate = savedDate || today

        // ถ้าข้ามวันแล้ว ล้างข้อมูลเก่า
        if (this.isNewDay) {
          this.clearLotRecords()
          this.lastAccessDate = today
          localStorage.setItem('lastAccessDate', today)
        } else if (savedRecords) {
          this.lotRecords = JSON.parse(savedRecords)
        }
      }
    },

    /**
     * ตั้งค่า summary data
     */
    setSummaryData(data: PoCheckFlowResponse[]) {
      this.summaryData = data
    },

    /**
     * อัพเดทวันที่ปัจจุบัน
     */
    updateCurrentDate() {
      const date = new Date()
      this.currentDate = date.toLocaleDateString('en-GB', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric'
      }).replace(/\//g, '-')
    },

    /**
     * ตั้งค่า Machine ปัจจุบัน
     */
    setCurrentMc(mc: string) {
      this.currentMc = mc
    },

    /**
     * ตั้งค่าสถานะ loading
     */
    setLoading(status: boolean) {
      this.isLoading = status
    },

    /**
     * ตั้งค่า error
     */
    setError(error: string | null) {
      this.error = error
    },

    /**
     * ล้าง error
     */
    clearError() {
      this.error = null
    },

    /**
     * เริ่มต้นการใช้งาน store
     */
    initialize() {
      this.updateCurrentDate()
      this.loadLotRecords()
    }
  }
})