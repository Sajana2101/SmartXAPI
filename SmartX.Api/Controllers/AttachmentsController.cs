using Microsoft.AspNetCore.Mvc;
using SmartX.Api.DTOs;
using SmartX.Api.Models;
using SmartX.Api.Repositories;
using SmartX.Api.Services;

namespace SmartX.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttachmentsController :
        ControllerBase
    {
        private readonly IAttachmentService
            _attachmentService;

        private readonly ISensorRepository
            _sensorRepository;

        public AttachmentsController(
            IAttachmentService attachmentService,
            ISensorRepository sensorRepository)
        {
            _attachmentService =
                attachmentService;

            _sensorRepository =
                sensorRepository;
        }

        [HttpGet("sensor/{sensorId:guid}")]
        public async Task<ActionResult<
            IReadOnlyList<AttachmentResponse>>>
            GetSensorAttachments(
                Guid sensorId)
        {
            Sensor? sensor =
                await _sensorRepository
                    .GetByIdAsync(sensorId);

            if (sensor is null)
            {
                return NotFound(new
                {
                    message =
                        "Sensor could not be found."
                });
            }

            IReadOnlyList<DeviceAttachment>
                attachments =
                    _attachmentService
                        .GetBySensor(sensorId);

            return Ok(
                attachments.Select(MapResponse));
        }

        [HttpPost("sensor/{sensorId:guid}")]
        [RequestSizeLimit(12_000_000)]
        public async Task<ActionResult<
            AttachmentResponse>>
            Upload(
                Guid sensorId,
                IFormFile file,
                CancellationToken cancellationToken)
        {
            Sensor? sensor =
                await _sensorRepository
                    .GetByIdAsync(sensorId);

            if (sensor is null)
            {
                return NotFound(new
                {
                    message =
                        "Sensor could not be found."
                });
            }

            try
            {
                DeviceAttachment attachment =
                    await _attachmentService
                        .UploadAsync(
                            sensorId,
                            file,
                            cancellationToken);

                return Ok(
                    MapResponse(attachment));
            }
            catch (
                InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpGet("{id:guid}/download")]
        public async Task<IActionResult>
            Download(
                Guid id,
                CancellationToken cancellationToken)
        {
            DeviceAttachment? attachment =
                _attachmentService
                    .GetById(id);

            if (attachment is null)
            {
                return NotFound(new
                {
                    message =
                        "Attachment could not be found."
                });
            }

            byte[] decryptedFile =
                await _attachmentService
                    .DownloadAsync(
                        id,
                        cancellationToken);

            return File(
                decryptedFile,
                attachment.ContentType,
                attachment.OriginalFileName);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult>
            Delete(Guid id)
        {
            bool deleted =
                await _attachmentService
                    .DeleteAsync(id);

            if (!deleted)
            {
                return NotFound(new
                {
                    message =
                        "Attachment could not be found."
                });
            }

            return NoContent();
        }

        private static AttachmentResponse
            MapResponse(
                DeviceAttachment attachment)
        {
            return new AttachmentResponse
            {
                Id = attachment.Id,
                SensorId =
                    attachment.SensorId,
                FileName =
                    attachment.OriginalFileName,
                ContentType =
                    attachment.ContentType,
                FileSizeBytes =
                    attachment.FileSizeBytes,
                UploadedAtUtc =
                    attachment.UploadedAtUtc,
                EncryptedAtRest =
                    attachment.EncryptedAtRest
            };
        }
    }
}