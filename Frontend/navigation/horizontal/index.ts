// horizontal.ts
export default () => {
  const roleName = useCookie('roleName').value
  const sectionName = decodeURIComponent(useCookie('sectionName').value || '')

  console.log("role", roleName);
  console.log("section", sectionName);
  
  
  if (!roleName) return []

  return [
    {
      title: 'Backend',
      icon: { icon: 'tabler-database' },
      children: [
        ...(roleName === 'admin' 
        ? [{
          title: 'INA web Tracking program',
          to: { name: 'ina-page' },
          icon: { icon: 'tabler-smart-home' },
        }] : []),
      ],
    },
    {
      title: 'Frontend',
      icon: { icon: 'tabler-database' },
      children: [
        ...(roleName === 'admin' || sectionName === 'OE-FP_Scribing & Chamfering'
          ? [{
              title: 'Register drawing and control',
              to: { name: 'registerdac' },
              icon: { icon: 'tabler-smart-home' },
            }]
          : []),
        {
          title: 'Polishing automatic control chart display',
          to: { name: 'pol-page' },
          icon: { icon: 'tabler-smart-home' },
        },
        {
          title: 'Polishing CheckFlow',
          to: { name: 'check-po' },
          icon: { icon: 'tabler-smart-home' },
        },
      ],
    },
    {
      title: 'Lapping',
      icon: { icon: 'tabler-database' },
      children: [
        ...(roleName === 'admin' || sectionName === 'OE-FP_Lapping'
          ? [{
              title: 'Lapping Material Control',
              to: { name: 'lapping_mat' },
              icon: { icon: 'tabler-smart-home' },
            }]
          : []),
        ...(roleName === 'admin' || sectionName === 'OE-FP_Lapping'
          ? [{
              title: 'QA word Checker',
              to: { name: 'qa-page' },
              icon: { icon: 'tabler-smart-home' },
            }]
          : []),
      ],
    },
    {
      title: 'QA',
      icon: { icon: 'tabler-database' },
      children: [
        ...(roleName === 'admin' || sectionName === 'OE-QA_Quality Assurance'
          ? [{
              title: 'QA word Checker',
              to: { name: 'qa-page' },
              icon: { icon: 'tabler-smart-home' },
            }]
          : []),
        ...(roleName === 'admin' || sectionName === 'OE-QA_Quality Assurance'
          ? [{
              title: 'Substance Master',
              to: { name: 'substance-master' },
              icon: { icon: 'tabler-smart-home' },
            }]
          : []),
        ...(roleName === 'admin' || sectionName === 'OE-QA_Quality Assurance'
          ? [{
              title: 'SVHC Master',
              to: { name: 'svhc-master' },
              icon: { icon: 'tabler-smart-home' },
            }]
          : []),
      ],
    },
  ]
}