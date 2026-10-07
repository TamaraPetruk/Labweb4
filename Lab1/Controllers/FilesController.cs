using Microsoft.AspNetCore.Mvc;

namespace Lab1.Controllers;

public class FilesController : Controller
{
    private readonly IWebHostEnvironment _environment;

    public FilesController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult ListFiles()
    {
        var filesDirectory = Path.Combine(
            _environment.WebRootPath,
            "Files");

        Directory.CreateDirectory(filesDirectory);

        var files = Directory
            .GetFiles(filesDirectory)
            .Select(Path.GetFileName)
            .Where(fileName => !string.IsNullOrEmpty(fileName))
            .OrderBy(fileName => fileName)
            .ToList();

        return View(files);
    }

    [HttpGet]
    public IActionResult ViewFile(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return NotFound();
        }

        var filesDirectory = Path.Combine(
            _environment.WebRootPath,
            "Files");

        var safeFileName = Path.GetFileName(fileName);
        var filePath = Path.Combine(filesDirectory, safeFileName);

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        ViewBag.FileName = safeFileName;

        var extension = Path.GetExtension(safeFileName)
            .ToLowerInvariant();

        if (extension == ".txt")
        {
            ViewBag.FileContent =
                System.IO.File.ReadAllText(filePath);
        }

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return RedirectToAction(nameof(Index));
        }

        var filesDirectory = Path.Combine(
            _environment.WebRootPath,
            "Files");

        Directory.CreateDirectory(filesDirectory);

        var fileName = Path.GetFileName(file.FileName);
        var filePath = Path.Combine(filesDirectory, fileName);

        await using var stream = new FileStream(
            filePath,
            FileMode.Create);

        await file.CopyToAsync(stream);

        TempData["UploadSuccess"] =
            "File uploaded successfully!";

        return RedirectToAction(nameof(Index));
    }
}