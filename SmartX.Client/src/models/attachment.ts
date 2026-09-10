export interface DeviceAttachment {
  id: string
  sensorId: string
  fileName: string
  contentType: string
  fileSizeBytes: number
  uploadedAtUtc: string
  encryptedAtRest: boolean
}