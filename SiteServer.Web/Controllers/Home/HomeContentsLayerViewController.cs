﻿using System;
using System.Web.Http;
using SiteServer.CMS.Core;
using SiteServer.CMS.Database.Caches;

namespace SiteServer.API.Controllers.Home
{
    [RoutePrefix("home/contentsLayerView")]
    public class HomeContentsLayerViewController : ApiController
    {
        private const string Route = "";

        [HttpGet, Route(Route)]
        public IHttpActionResult Get()
        {
            try
            {
                var rest = new Rest(Request);

                var siteId = rest.GetQueryInt("siteId");
                var channelId = rest.GetQueryInt("channelId");
                var contentId = rest.GetQueryInt("contentId");

                if (!rest.IsUserLoggin ||
                    !rest.UserPermissionsImpl.HasChannelPermissions(siteId, channelId,
                        ConfigManager.ChannelPermissions.ContentView))
                {
                    return Unauthorized();
                }

                var siteInfo = SiteManager.GetSiteInfo(siteId);
                if (siteInfo == null) return BadRequest("无法确定内容对应的站点");

                var channelInfo = ChannelManager.GetChannelInfo(siteId, channelId);
                if (channelInfo == null) return BadRequest("无法确定内容对应的栏目");

                var contentInfo = ContentManager.GetContentInfo(siteInfo, channelInfo, contentId);
                if (contentInfo == null) return BadRequest("无法确定对应的内容");

                contentInfo.Load(new
                {
                    CheckState =
                        CheckManager.GetCheckState(siteInfo, contentInfo)
                });

                var channelName = ChannelManager.GetChannelNameNavigation(siteId, channelId);

                var attributes = ChannelManager.GetContentsColumns(siteInfo, channelInfo, true);

                return Ok(new
                {
                    Value = contentInfo,
                    ChannelName = channelName,
                    Attributes = attributes
                });
            }
            catch (Exception ex)
            {
                LogUtils.AddErrorLog(ex);
                return InternalServerError(ex);
            }
        }
    }
}
