using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using _521_Project_3.Data;
using _521_Project_3.Models;
using System.Text.Json;
using System.Net;
using VaderSharp2;
using System.Web;

namespace _521_Project_3.Controllers
{
    public class ActorsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ActorsController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> GetActorPhoto(int id)
        {
            var Actor = await _context.Actor
                .FirstOrDefaultAsync(m => m.Id == id);
            if (Actor == null)
            {
                return NotFound();
            }
            var imageData = Actor.Photo;
            return File(imageData, "image/jpg");
        }

        // GET: Actors
        public async Task<IActionResult> Index()
        {
            return View(await _context.Actor.ToListAsync());
        }

        // GET: Actors/Details/5
        public async Task<IActionResult> Details(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actor
                .FirstOrDefaultAsync(m => m.Id == id);
            if (actor == null)
            {
                return NotFound();
            }

            ActorDetailsVM actorDetailsVM = new ActorDetailsVM();

            actorDetailsVM.Actor = actor;
            var queryText = actor.Name;
            var json = "";
            using (WebClient wc = new WebClient())
            {
                //fake like you are a "real" web browser
                wc.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                json = wc.DownloadString("https://www.reddit.com/search.json?limit=100&q=" + HttpUtility.UrlEncode(queryText));
            }
            var textToExamine = new List<string>();
            JsonDocument doc = JsonDocument.Parse(json);
            // Navigate to the "data" object
            JsonElement dataElement = doc.RootElement.GetProperty("data");
            // Navigate to the "children" array
            JsonElement childrenElement = dataElement.GetProperty("children");
            foreach (JsonElement child in childrenElement.EnumerateArray())
            {
                if (child.TryGetProperty("data", out JsonElement data))
                {
                    if (data.TryGetProperty("selftext", out JsonElement selftext))
                    {
                        string selftextValue = selftext.GetString();
                        if (!string.IsNullOrEmpty(selftextValue)) { textToExamine.Add(selftextValue); }
                        else if (data.TryGetProperty("title", out JsonElement title)) //use title if text is empty
                        {
                            string titleValue = title.GetString();
                            if (!string.IsNullOrEmpty(titleValue)) { textToExamine.Add(titleValue); }
                        }
                    }
                }
            }
            var analyzer = new SentimentIntensityAnalyzer();
            int validResults = 0;
            double resultsTotal = 0;
            foreach(string textValue in textToExamine)
            {
                var results = analyzer.PolarityScores(textValue);
                if(results.Compound != 0)
                {
                    resultsTotal += results.Compound;
                    validResults++;
                }

            }
            double avgResult = Math.Round(resultsTotal / validResults, 2);
            actorDetailsVM.Sentiment = avgResult.ToString();// + ", " + CategorizeSentiment(avgResult);

            var movies = new List<Movie>();
            ////Option 1

            movies = await (from movie in _context.Movie
                            join am in _context.ActorMovie on movie.Id equals am.MovieId
                            where am.ActorId == id
                            select movie)
                            .ToListAsync();

            ////Option 2
            //actors = await _context.ActorMovie.Where(am => am.MovieID == id)
            //                                .Include(a => a.Actor)
            //                                .Select(a => a.Actor)
            //                                .ToListAsync();

            actorDetailsVM.Movies = movies;

            return View(actorDetailsVM);

        }


        // GET: Actors/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Actors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Gender,Age,IMDBLink,Photo")] Actor actor, IFormFile Photo)
        {
            ModelState.Remove(nameof(actor.Photo));
            if (ModelState.IsValid)
            {
                if (Photo != null && Photo.Length > 0)
                {
                    var memoryStream = new MemoryStream();
                    await Photo.CopyToAsync(memoryStream);
                    actor.Photo = memoryStream.ToArray();

                }
                else
                {
                    actor.Photo= new byte[0];
                }
                _context.Add(actor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(actor);
        }

        // GET: Actors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actor.FindAsync(id);
            if (actor == null)
            {
                return NotFound();
            }
            return View(actor);
        }

        // POST: Actors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Gender,Age,IMDBLink,Photo")] Actor actor, IFormFile Photo)
        {
            if (id != actor.Id)
            {
                return NotFound();
            }
            ModelState.Remove(nameof(actor.Photo));

            Actor existingActor = _context.Actor.AsNoTracking().FirstOrDefault(m => m.Id == actor.Id);

            if (Photo != null && Photo.Length > 0)
            {
                var memoryStream = new MemoryStream();
                await Photo.CopyToAsync(memoryStream);
                actor.Photo = memoryStream.ToArray();
            }
            else if (existingActor == null)
            {
                actor.Photo = existingActor.Photo;
            }
            else
            {
                actor.Photo = new byte[0];
            }
            //not sure if the below code is right because he didnt freaking scroll down and the stupid code is not there like he says it is
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(actor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ActorExists(actor.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(actor);
        }

        // GET: Actors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actor
                .FirstOrDefaultAsync(m => m.Id == id);
            if (actor == null)
            {
                return NotFound();
            }

            return View(actor);
        }

        // POST: Actors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var actor = await _context.Actor.FindAsync(id);
            if (actor != null)
            {
                _context.Actor.Remove(actor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ActorExists(int id)
        {
            return _context.Actor.Any(e => e.Id == id);
        }
    }
}
