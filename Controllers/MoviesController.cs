using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using _521_Project_3.Data;
using _521_Project_3.Models;
using System.Net;
using System.Text.Json;
using System.Web;
using VaderSharp2;

namespace _521_Project_3.Controllers
{

    public class MoviesController : Controller
    {
        public static readonly HttpClient client = new HttpClient();


        public MoviesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> GetMovieImage(int id)
        {
            var Movie = await _context.Movie
                .FirstOrDefaultAsync(m => m.Id == id);
            if (Movie == null)
            {
                return NotFound();
            }
            var imageData = Movie.MovieImage;
            return File(imageData, "image/jpg");
        }
        private readonly ApplicationDbContext _context;



        // GET: Movies
        public async Task<IActionResult> Index()
        {
            return View(await _context.Movie.ToListAsync());
        }
        public static async Task<List<string>> SearchWikipediaAsync(string searchQuery)
        {
            string baseUrl = "https://en.wikipedia.org/w/api.php";
            string url = $"{baseUrl}?action=query&list=search&srlimit=100&srsearch={Uri.EscapeDataString(searchQuery)}&format=json";
            List<string> textToExamine = new List<string>();
            try
            {
                //Ask WikiPedia for a list of pages that relate to the query
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                var jsonDocument = JsonDocument.Parse(responseBody);
                var searchResults = jsonDocument.RootElement.GetProperty("query").GetProperty("search");
                foreach (var item in searchResults.EnumerateArray())
                {
                    var pageId = item.GetProperty("pageid").ToString();
                    //Ask WikiPedia for the text of each page in the query results
                    string pageUrl = $"{baseUrl}?action=query&pageids={pageId}&prop=extracts&explaintext&format=json";
                    HttpResponseMessage pageResponse = await client.GetAsync(pageUrl);
                    pageResponse.EnsureSuccessStatusCode();
                    string pageResponseBody = await pageResponse.Content.ReadAsStringAsync();
                    var jsonPageDocument = JsonDocument.Parse(pageResponseBody);
                    var pageContent = jsonPageDocument.RootElement.GetProperty("query").GetProperty("pages").GetProperty(pageId).GetProperty("extract").GetString();
                    textToExamine.Add(pageContent);
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
            return textToExamine;
        }
        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            MovieDetailsVM movieDetailsVM = new MovieDetailsVM();
            movieDetailsVM.Movie = movie;
            List<string> textToExamine = await SearchWikipediaAsync(movie.Title);

            var postSentiments = new List<PostSentiment>();

            var analyzer = new SentimentIntensityAnalyzer();
            int validResults = 0;
            double resultsTotal = 0;
            foreach(string textValue in textToExamine)
            {
                var results = analyzer.PolarityScores(textValue);
                var localSentiment = new PostSentiment();
                
                localSentiment.PostData = textValue;
                localSentiment.SentimentScore = Convert.ToDouble(results.Compound);
                // Console.WriteLine(localSentiment);
    
                postSentiments.Add(localSentiment);
                if(results.Compound != 0)
                {
                    
                    resultsTotal += results.Compound;
                    validResults++;
                }

            }

            movieDetailsVM.PostSentiments = postSentiments;
            
            double avgResult = Math.Round(resultsTotal / validResults, 2);
            movieDetailsVM.Sentiment = avgResult.ToString();// + ", " + CategorizeSentiment(avgResult);

            var actors = new List<Actor>();
            ////Option 1
            actors = await (from actor in _context.Actor
                            join am in _context.ActorMovie on actor.Id equals am.ActorId
                            where am.MovieId == id
                            select actor)
                            .ToListAsync();

            ////Option 2
            //actors = await _context.ActorMovie.Where(am => am.MovieID == id)
            //                                .Include(a => a.Actor)
            //                                .Select(a => a.Actor)
            //                                .ToListAsync();

            movieDetailsVM.Actors = actors;

            return View(movieDetailsVM);
        }

        // GET: Movies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Movies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Genre,YearReleased,IMDBLink")] Movie movie, IFormFile MovieImage)
        {
            ModelState.Remove(nameof(movie.MovieImage));
            if (ModelState.IsValid)
            {
                if(MovieImage != null && MovieImage.Length > 0)
                {
                    var memoryStream = new MemoryStream();
                    await MovieImage.CopyToAsync(memoryStream);
                    movie.MovieImage = memoryStream.ToArray();
                }
                else
                {
                    movie.MovieImage = new byte[0];
                }
                _context.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }

        // POST: Movies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Genre,YearReleased,MovieImage")] Movie movie, IFormFile MovieImage)
        {
            if (id != movie.Id)
            {
                return NotFound();
            }
            ModelState.Remove(nameof(movie.MovieImage));

            Movie existingMovie = _context.Movie.AsNoTracking().FirstOrDefault(m => m.Id == movie.Id);

            if (MovieImage != null && MovieImage.Length > 0)
            {
                var memoryStream = new MemoryStream();
                await MovieImage.CopyToAsync(memoryStream);
                movie.MovieImage = memoryStream.ToArray();
            }else if(existingMovie == null)
            {
                movie.MovieImage = existingMovie.MovieImage;
            }
            else
            {
                movie.MovieImage = new byte[0];
            }
            //not sure if the below code is right because he didnt freaking scroll down and the stupid code is not there like he says it is
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.Id))
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
            return View(movie);
        }

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movie.FindAsync(id);
            if (movie != null)
            {
                _context.Movie.Remove(movie);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movie.Any(e => e.Id == id);
        }
    }
}
