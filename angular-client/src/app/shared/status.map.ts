export const StatusMap: Record<string, string> = {
  Pending: 'În așteptare',
  InProgress: 'Se prepară',
  Done: 'Finalizată',
  Completed: 'Finalizată',
  Cancelled: 'Anulată',
  Canceled: 'Anulată'
};

export function toRoStatus(eng: string): string {
  return StatusMap[eng?.trim()] ?? eng;
}
