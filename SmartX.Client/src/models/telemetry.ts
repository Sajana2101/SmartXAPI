export type TelemetryType =
  | 'float'
  | 'int'
  | 'bool'

export interface TelemetryPacket<T> {
  id: string
  sensorId: string
  metric: string
  value: T
  timestampUtc: string
}

export interface TelemetryRequest<T> {
  sensorId: string
  metric: string
  value: T
}