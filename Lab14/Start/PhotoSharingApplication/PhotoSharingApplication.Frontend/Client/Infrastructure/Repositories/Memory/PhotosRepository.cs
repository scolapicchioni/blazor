using PhotoSharingApplication.Shared.Entities;
using PhotoSharingApplication.Shared.Interfaces;

namespace PhotoSharingApplication.Frontend.Client.Infrastructure.Repositories.Memory;
public class PhotosRepository : IPhotosRepository
{
    private List<Photo> photos;
    private List<PhotoImage> photoImages;
    public PhotosRepository()
    {
        photos = new List<Photo> {
            new Photo {Id=1, Title = "One photo", Description = "Lorem ipsum dolor sit amen", CreatedDate = DateTime.Now.AddDays(-2) },
            new Photo {Id=2, Title = "Another photo", Description = "Some description" ,CreatedDate= DateTime.Now.AddDays(-1)},
            new Photo {Id=3, Title = "Yet another photo", Description = "More description here", CreatedDate= DateTime.Now }
          };
        photoImages = new List<PhotoImage> {
            new PhotoImage {Id=1, ImageMimeType="jpg"},
            new PhotoImage {Id=2, ImageMimeType = "gif"},
            new PhotoImage {Id=3, ImageMimeType = "png"}
        };
    }

    public Task<Photo?> CreateAsync(Photo photo)
    {
        photo.Id = photos.Max(p => p.Id) + 1;
        photos.Add(photo);
        return Task.FromResult(photo)!;
    }

    public Task<Photo?> FindAsync(int id) => Task.FromResult(photos.FirstOrDefault(p => p.Id == id));

    public Task<List<Photo>> GetPhotosAsync(int startIndex, int amount, CancellationToken cancellationToken) => Task.FromResult(photos.OrderByDescending(p => p.CreatedDate).ThenBy(p => p.Title).Skip(startIndex).Take(amount).ToList());

    public Task<int> GetPhotosCountAsync() => Task.FromResult(photos.Count);

    public Task<Photo?> RemoveAsync(int id)
    {
        Photo? photo = photos.FirstOrDefault(p => p.Id == id);
        if (photo != null) photos.Remove(photo);
        return Task.FromResult(photo);
    }

    public Task<Photo?> UpdateAsync(Photo photo)
    {
        Photo? oldPhoto = photos.FirstOrDefault(p => p.Id == photo.Id);
        if (oldPhoto != null)
        {
            oldPhoto.Title = photo.Title;
            oldPhoto.Description = photo.Description;
            oldPhoto.CreatedDate = photo.CreatedDate;
            oldPhoto.UserName = photo.UserName;
            if (oldPhoto.PhotoImage is null)
                oldPhoto.PhotoImage = new PhotoImage();
            oldPhoto.PhotoImage.PhotoFile = photo.PhotoImage?.PhotoFile;
            oldPhoto.PhotoImage.ImageMimeType = photo.PhotoImage?.ImageMimeType;
        }
        return Task.FromResult(oldPhoto);
    }

    public Task<Photo?> FindWithImageAsync(int id) => Task.FromResult(photos.FirstOrDefault(p => p.Id == id));

    public Task<PhotoImage?> GetImageAsync(int id) => Task.FromResult(photoImages.FirstOrDefault(p => p.Id == id));
}