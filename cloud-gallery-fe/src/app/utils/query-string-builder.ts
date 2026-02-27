export function buildQueryString(filters: Record<string, any>): string {
  if (!filters) return '';
  const query = Object.entries(filters)
    .filter(([_, v]) => v !== undefined && v !== null)
    .map(([k, v]) => `${encodeURIComponent(k)}=${encodeURIComponent(String(v))}`)
    .join('&');
  return query ? '?' + query : '';
}
