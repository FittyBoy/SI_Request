interface Status {
    id?: string;
    statusName?: string;
    ordinal?: string;
  }
  const statuses = ref<Status[]>([]);
  