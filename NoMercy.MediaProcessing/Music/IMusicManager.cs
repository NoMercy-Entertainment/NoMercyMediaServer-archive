using NoMercy.Database.Models;

namespace NoMercy.MediaProcessing.Music;

public interface IMusicManager
{
    Task AddMusicAsync(int id, Library library);
    Task UpdateMusicAsync(int id, Library library);
    Task RemoveMusicAsync(int id);
}