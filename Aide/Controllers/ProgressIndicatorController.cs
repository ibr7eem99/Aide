using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Aide.Controllers
{
    public class ProgressIndicatorController : Controller
    {
        private static IDictionary<Guid, int> tasks = new Dictionary<Guid, int>();
        [HttpPost]
        public IActionResult Start()
        {
            Guid taskId = Guid.NewGuid();
            tasks.Add(taskId, 0);
            Task.Factory.StartNew(() =>
            {
                for (var i = 0; i <= 100; i++)
                {
                    tasks[taskId] = i; // update task progress
                    Console.WriteLine($"task Id = {taskId} = {i}");
                    Thread.Sleep(900);
                }
                tasks.Remove(taskId);
            });
            Console.WriteLine("return");
            return Json(taskId);
        }
        [HttpPost]
        public ActionResult Progress(Guid id)
        {
            return Json(tasks.Keys.Contains(id) ? tasks[id] : 100);
        }
    }
}
