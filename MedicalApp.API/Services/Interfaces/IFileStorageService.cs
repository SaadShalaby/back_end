namespace MedicalApp.API.Services.Interfaces
{
    public interface IFileStorageService
    {
        /// <summary>Upload a profile/avatar image — returns public URL</summary>
        Task<string> UploadFileAsync(IFormFile? file);

        /// <summary>Save any attachment (voice, image, file) to a sub-folder — returns public URL</summary>
        Task<string> SaveFileAsync(IFormFile file, string folder);

        /// <summary>Delete a file by its relative URL</summary>
        Task DeleteFileAsync(string fileUrl);
    }
}
