export default () => {
  const roleName = useCookie('roleName').value
  const sectionName = decodeURIComponent(useCookie('sectionName').value || '')

  if (!roleName) return []

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
      title: 'Lapping',
      icon: { icon: 'tabler-layers-difference' },
      children: [
        ...(roleName === 'admin' || sectionName === 'OE-FP_Lapping'
          ? [{
              title: 'Material Control',
              to: { name: 'lapping' },
              icon: { icon: 'tabler-box' },
            }]
          : []),
      ],
    },
    {
      title: 'QA',
      icon: { icon: 'tabler-microscope' },
      children: [
        ...(roleName === 'admin' || sectionName === 'OE-QA_Quality Assurance' || sectionName === 'OE-FP_Lapping'
          ? [{
              title: 'QA Word Checker',
              to: { name: 'qa' },
              icon: { icon: 'tabler-search' },
            }]
          : []),
        ...(roleName === 'admin' || sectionName === 'OE-QA_Quality Assurance'
          ? [{
              title: 'QA Chart Display',
              to: { name: 'qa-chart' },
              icon: { icon: 'tabler-chart-bar' },
            }]
          : []),
        ...(roleName === 'admin' || sectionName === 'OE-QA_Quality Assurance'
          ? [{
              title: 'Substance Master',
              to: { name: 'master-substance' },
              icon: { icon: 'tabler-flask' },
            }]
          : []),
        ...(roleName === 'admin' || sectionName === 'OE-QA_Quality Assurance'
          ? [{
              title: 'SVHC Master',
              to: { name: 'master-svhc' },
              icon: { icon: 'tabler-alert-triangle' },
            }]
          : []),
      ],
    },
    {
      title: 'Master Data',
      icon: { icon: 'tabler-database' },
      children: [
        ...(roleName === 'admin'
          ? [{
              title: 'Material Receive',
              to: { name: 'master-material-receive' },
              icon: { icon: 'tabler-truck' },
            }]
          : []),
      ],
    },
    {
      title: 'Request',
      icon: { icon: 'tabler-file-text' },
      children: [
        ...(roleName === 'admin'
          ? [{
              title: 'INA Tracking',
              to: { name: 'request-ina-page' },
              icon: { icon: 'tabler-smart-home' },
            }]
          : []),
        ...(roleName === 'admin' || sectionName === 'OE-FP_Scribing & Chamfering'
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
