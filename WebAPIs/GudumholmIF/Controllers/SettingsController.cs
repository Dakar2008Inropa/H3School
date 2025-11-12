using GudumholmIF.Models;
using GudumholmIF.Models.Application;
using GudumholmIF.Models.DTOs.Settings;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace GudumholmIF.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class SettingsController : ControllerBase
    {
        private readonly ClubContext db;

        public SettingsController(ClubContext db)
        {
            db = db ?? throw new ArgumentNullException(nameof(db));
            this.db = db;
        }

        [HttpGet]
        public async Task<ActionResult<SettingsDto>> Get(CancellationToken ct)
        {
            ApplicationSetting entity = await db.AppSettings.FirstOrDefaultAsync(ct);
            if (entity == null)
            {
                entity = new ApplicationSetting
                {
                    PassiveAdultAnnualFee = 400m,
                    PassiveChildAnnualFee = 200m,
                    ApiKey = string.Empty
                };
                db.AppSettings.Add(entity);

                await db.SaveChangesAsync(ct);
            }

            SettingsDto result = entity.Adapt<SettingsDto>();

            return Ok(result);
        }

        [HttpPut]
        public async Task<ActionResult<SettingsDto>> Update([FromBody] SettingsDto dto, CancellationToken ct)
        {
            ApplicationSetting entity = await db.AppSettings.FirstOrDefaultAsync(ct);
            if (entity == null)
            {
                entity = new ApplicationSetting();
                db.AppSettings.Add(entity);
            }

            entity.PassiveAdultAnnualFee = dto.PassiveAdultAnnualFee;
            entity.PassiveChildAnnualFee = dto.PassiveChildAnnualFee;

            await db.SaveChangesAsync(ct);

            SettingsDto result = entity.Adapt<SettingsDto>();

            return Ok(result);
        }

        [HttpGet("api-key")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<ApiKeyDto>> GetApiKey(CancellationToken ct)
        {
            ApplicationSetting entity = await db.AppSettings.FirstOrDefaultAsync(ct);
            if (entity == null)
            {
                entity = new ApplicationSetting
                {
                    PassiveAdultAnnualFee = 400m,
                    PassiveChildAnnualFee = 200m
                };
                db.AppSettings.Add(entity);
            }

            if (string.IsNullOrWhiteSpace(entity.ApiKey))
            {
                entity.ApiKey = GenerateApiKey();
                await db.SaveChangesAsync(ct);
            }

            ApiKeyDto dto = new ApiKeyDto { ApiKey = entity.ApiKey };
            return Ok(dto);
        }

        [HttpPost("api-key/rotate")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<ApiKeyDto>> RotateApiKey(CancellationToken ct)
        {
            ApplicationSetting entity = await db.AppSettings.FirstOrDefaultAsync(ct);
            if (entity == null)
            {
                entity = new ApplicationSetting
                {
                    PassiveAdultAnnualFee = 400m,
                    PassiveChildAnnualFee = 200m
                };
                db.AppSettings.Add(entity);
            }

            entity.ApiKey = GenerateApiKey();
            await db.SaveChangesAsync(ct);

            ApiKeyDto dto = new ApiKeyDto { ApiKey = entity.ApiKey };
            return Ok(dto);
        }

        private static string GenerateApiKey()
        {
            byte[] bytes = RandomNumberGenerator.GetBytes(32);
            string base64 = Convert.ToBase64String(bytes);
            return base64.Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }
    }
}