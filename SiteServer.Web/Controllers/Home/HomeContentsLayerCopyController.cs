﻿using System;
using System.Collections.Generic;
using System.Web.Http;
using SiteServer.CMS.Core;
using SiteServer.CMS.Core.Create;
using SiteServer.CMS.Core.Enumerations;
using SiteServer.CMS.Database.Caches;
using SiteServer.Utils;

namespace SiteServer.API.Controllers.Home
{
    [RoutePrefix("home/contentsLayerCopy")]
    public class HomeContentsLayerCopyController : ApiController
    {
        private const string Route = "";
        private const string RouteGetChannels = "actions/getChannels";

        [HttpGet, Route(Route)]
        public IHttpActionResult GetConfig()
        {
            try
            {
                var rest = new Rest(Request);

                var siteId = rest.GetQueryInt("siteId");
                var channelId = rest.GetQueryInt("channelId");
                var contentIdList = TranslateUtils.StringCollectionToIntList(rest.GetQueryString("contentIds"));

                if (!rest.IsUserLoggin ||
                    !rest.UserPermissionsImpl.HasChannelPermissions(siteId, channelId,
                        ConfigManager.ChannelPermissions.ContentTranslate))
                {
                    return Unauthorized();
                }

                var siteInfo = SiteManager.GetSiteInfo(siteId);
                if (siteInfo == null) return BadRequest("无法确定内容对应的站点");

                var channelInfo = ChannelManager.GetChannelInfo(siteId, channelId);
                if (channelInfo == null) return BadRequest("无法确定内容对应的栏目");

                var retval = new List<Dictionary<string, object>>();
                foreach (var contentId in contentIdList)
                {
                    var contentInfo = ContentManager.GetContentInfo(siteInfo, channelInfo, contentId);
                    if (contentInfo == null) continue;

                    var dict = contentInfo.ToDictionary();
                    dict["checkState"] =
                        CheckManager.GetCheckState(siteInfo, contentInfo);
                    retval.Add(dict);
                }

                var sites = new List<object>();
                var channels = new List<object>();

                var siteIdList = rest.UserPermissionsImpl.GetSiteIdList();
                foreach (var permissionSiteId in siteIdList)
                {
                    var permissionSiteInfo = SiteManager.GetSiteInfo(permissionSiteId);
                    sites.Add(new
                    {
                        permissionSiteInfo.Id,
                        permissionSiteInfo.SiteName
                    });
                }

                var channelIdList = rest.UserPermissionsImpl.GetChannelIdList(siteInfo.Id,
                    ConfigManager.ChannelPermissions.ContentAdd);
                foreach (var permissionChannelId in channelIdList)
                {
                    var permissionChannelInfo = ChannelManager.GetChannelInfo(siteInfo.Id, permissionChannelId);
                    channels.Add(new
                    {
                        permissionChannelInfo.Id,
                        ChannelName = ChannelManager.GetChannelNameNavigation(siteInfo.Id, permissionChannelId)
                    });
                }

                return Ok(new
                {
                    Value = retval,
                    Sites = sites,
                    Channels = channels,
                    Site = siteInfo
                });
            }
            catch (Exception ex)
            {
                LogUtils.AddErrorLog(ex);
                return InternalServerError(ex);
            }
        }

        [HttpGet, Route(RouteGetChannels)]
        public IHttpActionResult GetChannels()
        {
            try
            {
                var rest = new Rest(Request);

                var siteId = rest.GetQueryInt("siteId");

                var channels = new List<object>();
                var channelIdList = rest.UserPermissionsImpl.GetChannelIdList(siteId,
                    ConfigManager.ChannelPermissions.ContentAdd);
                foreach (var permissionChannelId in channelIdList)
                {
                    var permissionChannelInfo = ChannelManager.GetChannelInfo(siteId, permissionChannelId);
                    channels.Add(new
                    {
                        permissionChannelInfo.Id,
                        ChannelName = ChannelManager.GetChannelNameNavigation(siteId, permissionChannelId)
                    });
                }

                return Ok(new
                {
                    Value = channels
                });
            }
            catch (Exception ex)
            {
                LogUtils.AddErrorLog(ex);
                return InternalServerError(ex);
            }
        }

        [HttpPost, Route(Route)]
        public IHttpActionResult Submit()
        {
            try
            {
                var rest = new Rest(Request);

                var siteId = rest.GetPostInt("siteId");
                var channelId = rest.GetPostInt("channelId");
                var contentIdList = TranslateUtils.StringCollectionToIntList(rest.GetPostString("contentIds"));
                var targetSiteId = rest.GetPostInt("targetSiteId");
                var targetChannelId = rest.GetPostInt("targetChannelId");
                var copyType = rest.GetPostString("copyType");

                if (!rest.IsUserLoggin ||
                    !rest.UserPermissionsImpl.HasChannelPermissions(siteId, channelId,
                        ConfigManager.ChannelPermissions.ContentTranslate))
                {
                    return Unauthorized();
                }

                var siteInfo = SiteManager.GetSiteInfo(siteId);
                if (siteInfo == null) return BadRequest("无法确定内容对应的站点");

                var channelInfo = ChannelManager.GetChannelInfo(siteId, channelId);
                if (channelInfo == null) return BadRequest("无法确定内容对应的栏目");

                foreach (var contentId in contentIdList)
                {
                    ContentUtility.Translate(siteInfo, channelId, contentId, targetSiteId, targetChannelId, ETranslateContentTypeUtils.GetEnumType(copyType));
                }

                rest.AddSiteLog(siteId, channelId, "复制内容", string.Empty);

                CreateManager.TriggerContentChangedEvent(siteId, channelId);

                return Ok(new
                {
                    Value = contentIdList
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
