// composables/useFlowOutApi.ts
export const useFlowOutApi = () => {
  const config = useRuntimeConfig()
  const baseURL = config.public.apiBase

  /**
   * ค้นหา LOT จากระบบ
   */
  const searchLot = async (lotNumber: string) => {
    try {
      const response = await $fetch('/api/SI25031/search-lot', {
        baseURL,
        method: 'POST',
        body: { lotNumber }
      })
      return response
    } catch (error: any) {
      throw error.data || error
    }
  }

  /**
   * บันทึก LOT เข้าระบบ
   */
  const saveLot = async (imobileLot: string, lotQty: number) => {
    try {
      const response = await $fetch('/api/SI25031/save-lot', {
        baseURL,
        method: 'POST',
        body: { imobileLot, lotQty }
      })
      return response
    } catch (error: any) {
      throw error.data || error
    }
  }

  /**
   * ดึงข้อมูล LOT ตาม MC และวัน
   */
  const getLotsByMc = async (mcNo: string, date?: string) => {
    try {
      const params: any = { mcNo }
      if (date) params.date = date

      const response = await $fetch('/api/SI25031/get-lots-by-mc', {
        baseURL,
        method: 'GET',
        params
      })
      return response
    } catch (error: any) {
      throw error.data || error
    }
  }

  /**
   * ดึงรายการ MC ทั้งหมด
   */
  const getMcList = async (date?: string) => {
    try {
      const params: any = {}
      if (date) params.date = date

      const response = await $fetch('/api/SI25031/get-mc-list', {
        baseURL,
        method: 'GET',
        params
      })
      return response
    } catch (error: any) {
      throw error.data || error
    }
  }

  /**
   * ดึงรายการ MC จาก ThRecord (MC ที่มีในวันนั้น)
   */
  const getMcListFromThRecord = async (date?: string) => {
    try {
      const params: any = {}
      if (date) params.date = date

      const response = await $fetch('/api/SI25031/get-mc-list-from-threcord', {
        baseURL,
        method: 'GET',
        params
      })
      return response
    } catch (error: any) {
      throw error.data || error
    }
  }

  /**
   * ✅ ดึงข้อมูล LOT ทั้งหมดตามวันจาก po_check_flow
   */
  const getAllLotsByDate = async (date?: string) => {
    try {
      const params: any = {}
      if (date) params.date = date

      const response = await $fetch('/api/SI25031/get-all-lots-by-date', {
        baseURL,
        method: 'GET',
        params
      })
      return response
    } catch (error: any) {
      throw error.data || error
    }
  }

  /**
   * ดึงข้อมูล LOT จาก CheckFlow (เดิม)
   */
  const getLotsFromCheckFlow = async (date?: string) => {
    try {
      const params: any = {}
      if (date) params.date = date

      const response = await $fetch('/api/SI25031/get-all-lots-by-date', {
        baseURL,
        method: 'GET',
        params
      })
      return response
    } catch (error: any) {
      throw error.data || error
    }
  }

  /**
   * ลบ LOT
   */
  const deleteLot = async (id: string) => {
    try {
      const response = await $fetch(`/api/SI25031/delete-lot/${id}`, {
        baseURL,
        method: 'DELETE'
      })
      return response
    } catch (error: any) {
      throw error.data || error
    }
  }

  /**
   * ดึงสถิติรายวัน
   */
  const getDailyStats = async (mcNo?: string, date?: Date) => {
    try {
      const params: any = {}
      if (mcNo) params.mcNo = mcNo
      if (date) params.date = date.toISOString()

      const response = await $fetch('/api/SI25031/get-daily-stats', {
        baseURL,
        method: 'GET',
        params
      })
      return response
    } catch (error: any) {
      throw error.data || error
    }
  }

  /**
   * ดึงข้อมูล LOT พร้อม Quantity
   */
  const getLotsWithQuantity = async (mcNo?: string, date?: Date) => {
    try {
      const params: any = {}
      if (mcNo) params.mcNo = mcNo
      if (date) params.date = date.toISOString()

      const response = await $fetch('/api/SI25031/get-lots-with-quantity', {
        baseURL,
        method: 'GET',
        params
      })
      return response
    } catch (error: any) {
      throw error.data || error
    }
  }

  /**
   * ดึงสรุป LOT ตาม MC
   */
  const getMcLotsSummary = async (mcNo: string) => {
    try {
      const response = await $fetch('/api/SI25031/get-mc-lots-summary', {
        baseURL,
        method: 'GET',
        params: { mcNo }
      })
      return response
    } catch (error: any) {
      throw error.data || error
    }
  }

  /**
   * ดึงสรุป LOT แยกตาม MC
   */
  const getLotsSummaryByMc = async (date?: Date) => {
    try {
      const params: any = {}
      if (date) params.date = date.toISOString()

      const response = await $fetch('/api/SI25031/get-lots-summary-by-mc', {
        baseURL,
        method: 'GET',
        params
      })
      return response
    } catch (error: any) {
      throw error.data || error
    }
  }

  /**
   * ดึง LOT ตาม MC จาก ThRecord
   */
  const getLotsByMcFromThRecord = async (mcNo: string, date?: string) => {
    try {
      const params: any = { mcNo }
      if (date) params.date = date

      const response = await $fetch('/api/SI25031/get-lots-by-mc-from-threcord', {
        baseURL,
        method: 'GET',
        params
      })
      return response
    } catch (error: any) {
      throw error.data || error
    }
  }

  return {
    searchLot,
    saveLot,
    getLotsByMc,
    getMcList,
    getMcListFromThRecord,
    getAllLotsByDate, // ✅ เพิ่ม
    getLotsFromCheckFlow,
    deleteLot,
    getDailyStats,
    getLotsWithQuantity,
    getMcLotsSummary,
    getLotsSummaryByMc,
    getLotsByMcFromThRecord
  }
}