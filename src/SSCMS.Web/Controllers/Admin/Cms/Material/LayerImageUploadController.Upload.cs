﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Enums;
using SSCMS.Utils;

namespace SSCMS.Web.Controllers.Admin.Cms.Material
{
    public partial class LayerImageUploadController
    {
        [RequestSizeLimit(long.MaxValue)]
        [HttpPost, Route(RouteUpload)]
        public async Task<ActionResult<UploadResult>> Upload([FromQuery] int siteId, [FromForm] IFormFile file)
        {
            var site = await _siteRepository.GetAsync(siteId);

            if (file == null)
            {
                return this.Error("请选择有效的文件上传");
            }

            var fileName = PathUtils.GetFileName(file.FileName);
            var extName = PathUtils.GetExtension(fileName);
            if (!_pathManager.IsImageExtensionAllowed(site, extName))
            {
                return this.Error("此图片格式已被禁止上传，请转换格式后上传!");
            }

            var materialFileName = PathUtils.GetMaterialFileName(fileName);
            var virtualDirectoryPath = PathUtils.GetMaterialVirtualDirectoryPath(UploadType.Image);

            var directoryPath = PathUtils.Combine(_pathManager.WebRootPath, virtualDirectoryPath);
            var filePath = PathUtils.Combine(directoryPath, materialFileName);

            await _pathManager.UploadAsync(file, filePath);
            await _pathManager.AddWaterMarkAsync(site, filePath);

            return new UploadResult
            {
                Name = fileName,
                Path = PageUtils.Combine(virtualDirectoryPath, materialFileName)
            };
        }
    }
}