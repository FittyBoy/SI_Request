// รับ roleName และ sectionName เป็น params แทนการเรียก useCookie ภายใน
// เพื่อป้องกัน recursive computed loop ใน layout
export default (roleName?: string | null, sectionName?: string) => {
  const role    = roleName   ?? ''
  const section = sectionName ?? ''

  if (!role) return []

  return [
    {
      title: 'Polishing',
      icon: { icon: 'tabler-circle-dot' },
      children: [
        {
          title: 'Polishing CheckFlow',
          to: { name: 'polishing-check-po' },
          icon: { icon: 'tabler-checklist' },
        },
        {
          title: 'Auto Control Chart',
          to: { name: 'polishing-pol-page' },
          icon: { icon: 'tabler-chart-line' },
        },
      ],
    },
    {
      title: 'QA',
      icon: { icon: 'tabler-microscope' },
      children: [
        ...(role === 'admin' || section === 'OE-QA_Quality Assurance' || section === 'OE-FP_Lapping'
          ? [{
              title: 'QA Word Checker',
              to: { name: 'qa' },
              icon: { icon: 'tabler-search' },
            }]
          : []),
        ...(role === 'admin' || section === 'OE-QA_Quality Assurance'
          ? [{
              title: 'Substance Master',
              to: { name: 'master-substance' },
              icon: { icon: 'tabler-flask' },
            }]
          : []),
        ...(role === 'admin' || section === 'OE-QA_Quality Assurance'
          ? [{
              title: 'SVHC Master',
              to: { name: 'master-svhc' },
              icon: { icon: 'tabler-alert-triangle' },
            }]
          : []),
      ],
    },
    {
      title: 'Request',
      icon: { icon: 'tabler-file-text' },
      children: [
        ...(role === 'admin'
          ? [{
              title: 'INA Tracking',
              to: { name: 'request-ina-page' },
              icon: { icon: 'tabler-smart-home' },
            }]
          : []),
        ...(role === 'admin' || section === 'OE-FP_Scribing & Chamfering'
          ? [{
              title: 'Register Drawing',
              to: { name: 'request-registerdac' },
              icon: { icon: 'tabler-pencil' },
            }]
          : []),
      ],
    },
  ]
}
