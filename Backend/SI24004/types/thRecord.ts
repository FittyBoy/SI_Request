export interface ThRecordDTO {
    lotId: number;
    lotPo: string;
    mcPo: string;
    noPo: string;
    memberId: string;
    status: string;
    dateProcess: Date;
    timeProcess: Date;
    thBefore: number | null;  // nullable
    avgTh: number | null;     // nullable
    processTime: number | null;  // nullable
    poRate: number | null;   // nullable
    thDif: number | null;    // nullable
    margin: number | null;   // nullable
    result: string;
    remark: string;
    imobileLot: string;
    imobileType: string;
    imobileSize: string;
    mcType: string;
    process: string;
    ca1In: (number | null)[];  // nullable array
    ca1Out: (number | null)[]; // nullable array
    ca2In: (number | null)[];  // nullable array
    ca2Out: (number | null)[]; // nullable array
    ca3In: (number | null)[];  // nullable array
    ca3Out: (number | null)[]; // nullable array
    ca4In: (number | null)[];  // nullable array
    ca4Out: (number | null)[]; // nullable array
    ca5In: (number | null)[];  // nullable array
    ca5Out: (number | null)[]; // nullable array
    thCin: number | null;   // nullable
    thCout1: number | null; // nullable
    thCout2: number | null; // nullable
    thCout3: number | null; // nullable
    thCout4: number | null; // nullable
    thCout5: number | null; // nullable
    hostname: string;
    ipAddress: string;
    laserMC: string;
    processStep: number | null; // nullable
    ca1In6: (number | null)[];  // nullable array
    ca1In7: (number | null)[];  // nullable array
    ca1In8: (number | null)[];  // nullable array
    ca1In9: (number | null)[];  // nullable array
    ca1In10: (number | null)[]; // nullable array
    ca1Out6: (number | null)[]; // nullable array
    ca1Out7: (number | null)[]; // nullable array
    ca1Out8: (number | null)[]; // nullable array
    ca1Out9: (number | null)[]; // nullable array
    ca1Out10: (number | null)[]; // nullable array
    ca2In6: (number | null)[];  // nullable array
    ca2In7: (number | null)[];  // nullable array
    ca3In6: (number | null)[];  // nullable array
    ca3In7: (number | null)[];  // nullable array
    ca4In6: (number | null)[];  // nullable array
    ca4In7: (number | null)[];  // nullable array
    ca5In6: (number | null)[];  // nullable array
    ca5In7: (number | null)[];  // nullable array
    thCin2: number | null;  // nullable
    thCin3: number | null;  // nullable
    thCin4: number | null;  // nullable
    thCin5: number | null;  // nullable
    ca2In8: (number | null)[];  // nullable array
    ca2In9: (number | null)[];  // nullable array
    ca2In10: (number | null)[]; // nullable array
    ca3In8: (number | null)[];  // nullable array
    ca3In9: (number | null)[];  // nullable array
    ca3In10: (number | null)[]; // nullable array
    ca4In8: (number | null)[];  // nullable array
    ca4In9: (number | null)[];  // nullable array
    ca4In10: (number | null)[]; // nullable array
    ca5In8: (number | null)[];  // nullable array
    ca5In9: (number | null)[];  // nullable array
    ca5In10: (number | null)[]; // nullable array
    program: string | null; // nullable (หากไม่ต้องการ สามารถลบออกได้)
}
