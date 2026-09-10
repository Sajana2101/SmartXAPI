export interface TelemetryDashboardSensor {
  sensorId: string
  deviceIdentifier: string
  category: string
  metric: string
  latestValue: string
  unit: string
  latestTimestampUtc: string | null
  isConnected: boolean
  isAnomaly: boolean
  statusMessage: string
  recentValues: number[]
}

export interface TelemetryDashboard {
  generatedAtUtc: string
  totalSensors: number
  connectedSensors: number
  anomalyCount: number
  sensors: TelemetryDashboardSensor[]
}