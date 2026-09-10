interface TelemetrySparklineProps {
  values: number[]
}

function TelemetrySparkline({
  values,
}: TelemetrySparklineProps) {
  if (values.length < 2) {
    return (
      <div className="sparkline-empty">
        Waiting for more telemetry...
      </div>
    )
  }

  const width = 300
  const height = 80
  const padding = 8

  const minimum =
    Math.min(...values)

  const maximum =
    Math.max(...values)

  const range =
    maximum - minimum || 1

  const points =
    values
      .map((value, index) => {
        const x =
          padding +
          (index /
            (values.length - 1)) *
            (width - padding * 2)

        const y =
          height -
          padding -
          ((value - minimum) /
            range) *
            (height - padding * 2)

        return `${x},${y}`
      })
      .join(' ')

  return (
    <svg
      className="sparkline"
      viewBox={`0 0 ${width} ${height}`}
      role="img"
      aria-label="Recent telemetry trend"
    >
      <polyline
        points={points}
        fill="none"
        stroke="currentColor"
        strokeWidth="3"
        strokeLinejoin="round"
        strokeLinecap="round"
      />
    </svg>
  )
}

export default TelemetrySparkline