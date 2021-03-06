﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Web.Model.Context;
using RedMan.DataAccess.IRepository;
using Web.Model.Entities;
using RedMan.DataAccess.Repository;
using RedMan.ViewModel;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using RedMan.Extensions;
using Microsoft.AspNetCore.Hosting;
using MarkdownSharp;


namespace RedMan.Controllers
{
    /// <summary>
    /// 话题控制器
    /// </summary>
    [Authorize]
    public class TopicController : Controller
    {
        #region - Private -
        private readonly IHostingEnvironment _env;
        private readonly ModelContext _context;
        private readonly IRepository<Topic> _topicRepo;
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<Reply> _replyRepo;
        private readonly IRepository<TopicCollect> _topicCollectRepo;
        #endregion

        /// <summary>
        /// 初始化话题控制器<see cref="TopicController"/>实例
        /// </summary>
        /// <param name="env">指定的当前寄宿环境实例</param>
        /// <param name="context">指定的模型上下文<see cref="ModelContext"/>实例</param>
        public TopicController(IHostingEnvironment env, ModelContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            this._env = env;
            this._context = context;
            this._userRepo = new Repository<User>(context);
            this._topicRepo = new Repository<Topic>(context);
            this._replyRepo = new Repository<Reply>(context);
            this._topicCollectRepo = new Repository<TopicCollect>(context);
        }

        /// <summary>
        /// 话题首页
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public async Task<IActionResult> Index(int id)
        {
            var topic = await this._topicRepo.FindAsync(p => p.TopicId == id && !p.Deleted);
            if (topic == null)
                throw new Exception("找不到此话题,或已被删除");
            topic.VisitCount += 1;
            await this._topicRepo.UpdateAsync(topic);
            var author = await this._userRepo.FindAsync(p => p.UserId == topic.AuthorId);
            if (author == null)
                throw new Exception("作者未找到");

            var topicViewModel = new TopicViewModel()
            {
                Tab = topic.Type,
                Title = topic.Title,
                Content = topic.Content,

                Topic = topic,
                Author = author,
                Html = topic.Html
            };

            if (User.Claims.Any(p => p.Type == ClaimTypes.Name))
            {
                var loginUser = await this._userRepo.FindAsync(p => p.Name == User.Identity.Name);
                topicViewModel.LoginUser = loginUser;
                topicViewModel.Collected = await this._topicCollectRepo.IsExistAsync(p => p.TopicId == topic.TopicId && p.UserId == loginUser.UserId);
            }

            return View(topicViewModel);
        }

        /// <summary>
        /// 发布话题
        /// </summary>
        /// <returns>发布话题页面</returns>
        public IActionResult Add()
        {
            ViewData["Error"] = false;
            ViewData["Action"] = "Add";
            return View(new TopicViewModel());
        }

        /// <summary>
        /// 发布话题
        /// </summary>
        /// <param name="model">指定发布的话题实例</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(TopicViewModel model)
        {
            ViewData["Action"] = "Add";
            ViewData["Error"] = true;
            model.Title = model.Title?.Trim();
            model.Content = model.Content?.Trim();
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var loginUser = await this._userRepo.FindAsync(p => p.Name == User.Identity.Name);
            if (loginUser == null)
            {
                ModelState.AddModelError("", "用户找不到");
                return View(model);
            }
            var topic = new Topic()
            {
                Title = model.Title,
                Content = model.Content,
                AuthorId = loginUser.UserId,
                CreateDateTime = DateTime.Now,
                Type = model.Tab
            };
            var result = await this._topicRepo.AddAsync(topic, false);
            loginUser.Topic_Count += 1;
            loginUser.Score += 5;
            result = await this._userRepo.UpdateAsync(loginUser);
            if (result)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("", "出现未知错误，发布失败，请稍后再试");
                return View(model);
            }
        }

        /// <summary>
        /// 编辑话题
        /// </summary>
        /// <param name="topidId">话题编号</param>
        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Error"] = false;
            var topic = await this._topicRepo.FindAsync(p => p.TopicId == id);
            if (topic == null)
                throw new Exception("此话题未找到，或已被删除");
            var loginUserName = User.Identity.Name;
            var loginUser = await this._userRepo.FindAsync(p => p.Name == loginUserName);

            var topicViewModel = new TopicViewModel()
            {
                Author = loginUser,
                Content = topic.Content,
                Title = topic.Title,
                Tab = topic.Type,
                LoginUser = loginUser,
                Topic = topic,
                TopicId = topic.TopicId
            };
            return View("Add", topicViewModel);
        }

        /// <summary>
        /// 编辑话题
        /// </summary>
        /// <param name="model">指定编辑话题的实例</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TopicViewModel model)
        {
            model.Title = model.Title?.Trim();
            model.Content = model.Content?.Trim();

            ViewData["Error"] = true;
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var loginUserName = User.Identity.Name;
            var loginUser = await this._userRepo.FindAsync(p => p.Name == loginUserName);
            if (loginUser == null)
            {
                ModelState.AddModelError("", "用户找不到");
                return View(model);
            }
            var topicOrign = await this._topicRepo.FindAsync(p => p.TopicId == model.TopicId);
            if (topicOrign == null)
                throw new Exception("找不到话题，或已被删除");
            topicOrign.Title = model.Title;
            topicOrign.Content = model.Content;
            topicOrign.Type = model.Tab;
            topicOrign.UpdateDateTime = DateTime.Now;

            var result = await this._topicRepo.UpdateAsync(topicOrign);
            if (result)
            {
                return new RedirectResult(Url.Content($"/Topic/Index/{topicOrign.TopicId}"));
            }
            else
            {
                ModelState.AddModelError("", "出现未知错误，编辑失败，请稍后再试");
                return View(model);
            }
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="file">文件</param>
        /// <returns>操作结果及相关信息</returns>
        public async Task<JsonResult> Upload(IFormFile file)
        {
            var fileSplit = file.FileName.Split('.');
            if (fileSplit.Length <= 1)
                return Json(new { success = false, message = "无法识别的扩展名" });
            var fileExtenions = "." + fileSplit[fileSplit.Length - 1];
            if (fileExtenions.ToLower() != ".jpg" && fileExtenions.ToLower() != ".png")
                return Json(new { success = false, message = "请上传 .jpg 或者 .png格式的图片" });
            var fileName = DateTime.Now.ToUnixTimestamp() + fileExtenions;
            var filePath = await new UploadFile(this._env).UploadImage(file, fileName);
            return Json(new { success = true, url = filePath });
        }

        /// <summary>
        /// 删除话题
        /// </summary>
        /// <param name="id">话题ID</param>
        /// <returns>操作结果</returns>
        [HttpPost]
        public async Task<JsonResult> Delete(int id)
        {
            var topic = await this._topicRepo.FindAsync(p => p.TopicId == id);
            if (topic == null)
                return Json(new { success = false, message = "找不到此话题" });
            else
            {
                topic.Deleted = true;
                var success = await this._topicRepo.UpdateAsync(topic, false);
                var user = await this._userRepo.FindAsync(p => p.UserId == topic.AuthorId);
                if (user != null)
                {
                    user.Topic_Count -= 1;
                    success = await this._userRepo.UpdateAsync(user);
                }
                if (success)
                    return Json(new { success = true, message = "删除成功" });
                else
                    return Json(new { success = false, message = "删除失败" });
            }
        }

        /// <summary>
        /// 添加到收藏
        /// </summary>
        /// <param name="id">话题ID</param>
        /// <returns>操作结果</returns>
        [HttpPost]
        public async Task<JsonResult> Collecte(int id)
        {
            var loginUserName = User.Identity.Name;
            var loginUser = await this._userRepo.FindAsync(p => p.Name == loginUserName);
            if (loginUser == null)
                return Json(new { status = "error" });
            loginUser.Collect_Topic_Count += 1;
            var topicCollect = new TopicCollect()
            {
                UserId = loginUser.UserId,
                TopicId = id,
                CreateDateTime = DateTime.Now
            };
            await this._userRepo.UpdateAsync(loginUser, false);
            var success = await this._topicCollectRepo.AddAsync(topicCollect);
            if (success)
                return Json(new { status = "success" });
            else
                return Json(new { status = "error" });
        }

        /// <summary>
        /// 删除收藏
        /// </summary>
        /// <param name="id">话题ID</param>
        /// <returns>操作结果</returns>
        [HttpPost]
        public async Task<IActionResult> CollecteDel(int id)
        {
            var loginUser = await this._userRepo.FindAsync(p => p.Name == User.Identity.Name);
            if (loginUser == null)
                return Json(new { status = "error" });
            loginUser.Collect_Topic_Count -= 1;
            await this._userRepo.UpdateAsync(loginUser, false);
            var success = await this._topicCollectRepo.DeleteAsync(p => p.UserId == loginUser.UserId && p.TopicId == id);
            if (success)
                return Json(new { status = "success" });
            else
                return Json(new { status = "error" });
        }

        /// <summary>
        /// 置顶
        /// </summary>
        /// <param name="id">话题ID</param>
        /// <returns>操作结果</returns>
        [HttpPost]
        public async Task<JsonResult> Top(int id)
        {
            var topic = await this._topicRepo.FindAsync(p => p.TopicId == id);
            if (topic == null)
                return Json(new { status = "error" });
            if (topic.Top)
            {
                topic.Top = false;
                if (await this._topicRepo.UpdateAsync(topic))
                    return Json(new { status = "cancel_top" });
                else
                    return Json(new { status = "error" });
            }
            else
            {
                topic.Top = true;
                if (await this._topicRepo.UpdateAsync(topic))
                    return Json(new { status = "top" });
                else
                    return Json(new { status = "error" });
            }
        }

        /// <summary>
        /// 设置为精华
        /// </summary>
        /// <param name="id">话题ID</param>
        /// <returns>操作结果</returns>
        [HttpPost]
        public async Task<JsonResult> Good(int id)
        {
            var topic = await this._topicRepo.FindAsync(p => p.TopicId == id);
            if (topic == null)
                return Json(new { status = "error" });
            if (topic.Good)
            {
                topic.Good = false;
                if (await this._topicRepo.UpdateAsync(topic))
                    return Json(new { status = "cancel_good" });
                else
                    return Json(new { status = "error" });
            }
            else
            {
                topic.Good = true;
                if (await this._topicRepo.UpdateAsync(topic))
                    return Json(new { status = "good" });
                else
                    return Json(new { status = "error" });
            }

        }
    }
}
