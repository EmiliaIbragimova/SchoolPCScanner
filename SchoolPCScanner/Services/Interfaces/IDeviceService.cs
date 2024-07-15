using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using SchoolPCScanner.Models;

namespace SchoolPCScanner.Services.Interfaces
{
    public interface IDeviceService
    {
        Task CreateDeviceAsync(Device device);
        Task<Device> GetDeviceByIdAsync(int? id);
        Task<Device> GetDeviceByBarcodeOrSerieNumberAsync(string identifier);
        Task<Device> GetDeviceByBarcodeAsync(string barcode);
        //Task<Device> GetDeviceBySerieNumberAsync(string serienumber);
        Task<List<Device>> GetDevicesByStudentIdAsync(int? id);
        Task UpdateDeviceAsync(Device device);
        Task<IQueryable<Device>> GetAllDevicesAsync();
        Task DeleteDeviceAsync(Device device);
        Device CreateDeviceCopy(Device device);
        string GetDisplayName(Enum value);
    }
}
