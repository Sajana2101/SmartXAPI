
export interface HistoricalTelemetryEntry {
  id: string
  sensorId: string
  metric: string
  value: string
  valueType: string
  timestampUtc: string
}

export interface TelemetryQueueStatus {
  pendingCount: number
  processedCount: number
}

export interface TelemetryHistoryCount {
  count: number
}
