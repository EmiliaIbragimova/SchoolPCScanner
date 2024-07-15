using Microsoft.EntityFrameworkCore;
using SchoolPCScanner.Models;
using SchoolPCScanner.Services.Interfaces;
using SchoolPCScanner.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace SchoolPCScanner.Services
{
    public class DeviceService : IDeviceService
    {
        private readonly SchoolPCScannerDbContext _context;
        private readonly ISupplierService _supplierService;

        public DeviceService(SchoolPCScannerDbContext context, ISupplierService supplierService)
        {
            _context = context;
            _supplierService = supplierService;
        }

        public async Task CreateDeviceAsync(Device device)
        {
            try
            {
                // nieuw apparaat als beschikbaar toevoegen
                _context.Devices.Add(device);
                await _context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                throw new Exception("DeviceService > CreateDeviceAsync: An error occurred while creating the device", ex);
            }
        }

        public async Task<Device> GetDeviceByIdAsync(int? id)
        {
            try
            {
                return await _context.Devices
            .Include(d => d.Logs)
            .Include(d => d.Supplier)
            .Include(d => d.Student) // Inclusief studentgegevens
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
                //return await _context.Devices.Include(d => d.Logs).FirstOrDefaultAsync(d => d.Id == id);
            }
            catch (Exception ex)
            {
                throw new Exception("DeviceService > GetDeviceByIdAsync: An error occurred while retrieving device by id", ex);
            }
        }

        public async Task<Device> GetDeviceByBarcodeOrSerieNumberAsync(string identifier)
        {
            try
            {
                return await _context.Devices.FirstOrDefaultAsync(d => d.Barcode == identifier || d.Serienumber == identifier && !d.IsDeleted);

            }
            catch (Exception ex)
            {
                throw new Exception("DeviceService > GetDeviceByBarcodeAsync: An error occurred while retrieving device by barcode", ex);
            }
        }

        public async Task<Device> GetDeviceByBarcodeAsync(string barcode)
        {
            try
            {
                return await _context.Devices.FirstOrDefaultAsync(d => d.Barcode == barcode);

            }
            catch (Exception ex)
            {
                throw new Exception("DeviceService > GetDeviceByBarcodeAsync: An error occurred while retrieving device by barcode", ex);
            }
        }

        //public async Task<Device> GetDeviceBySerieNumberAsync(string serienumber)
        //{
        //    try
        //    {
        //        return await _context.Devices.FirstOrDefaultAsync(d => d.Serienumber == serienumber);

        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("DeviceService > GetDeviceBySerialNumberAsync: An error occurred while retrieving device by serial number", ex);
        //    }
        //}

        public async Task<List<Device>> GetDevicesByStudentIdAsync(int? id)
        {
            try
            {
                return await _context.Devices.Where(d => d.StudentId == id && !d.IsDeleted).ToListAsync();

            }
            catch (Exception ex)
            {
                throw new Exception("DeviceService > GetDevicesByStudentIdAsync: An error occurred while retrieving devices by student id", ex);
            }
        }

        public async Task UpdateDeviceAsync(Device device)
        {
            try
            {
                _context.Devices.Update(device);
                await _context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                throw new Exception("DeviceService > UpdateDeviceAsync: An error occurred while updating the device", ex);
            }
        }

        public async Task<IQueryable<Device>> GetAllDevicesAsync()
        {
            try
            {
                return _context.Devices
                    .Include(d => d.Student)
                    .Include(d => d.Supplier)
                    .Where(d => !d.IsDeleted)
                    .OrderByDescending(d => d.Id);  // Sort by Id in descending order

            }
            catch (Exception ex)
            {
                throw new Exception("DeviceService > GetAllDevicesAsync: An error occurred while retrieving all devices", ex);
            }
        }

        public async Task DeleteDeviceAsync(Device device)
        {
            try
            {
                    device.IsDeleted = true;
                    _context.Devices.Update(device);
                    //_context.Devices.Remove(device);
                    await _context.SaveChangesAsync();
                
        
            }
            catch (Exception ex)
            {
                throw new Exception("DeviceService > DeleteDeviceAsync: An error occurred while removing a device", ex);
            }
        }

        public Device CreateDeviceCopy(Device device)
        {
            // Maak een nieuwe instantie van het Device-object en kopieer de waarden van het meegegeven apparaat
            try
            {
                return new Device
                {
                    Id = device.Id,
                    Serienumber = device.Serienumber,
                    SupplierId = device.SupplierId,
                    Supplier = device.Supplier,
                    Type = device.Type,
                    Student = device.Student,
                    StudentId = device.StudentId,
                    Barcode = device.Barcode,
                    Status = device.Status
                };
            }
            catch (Exception ex)
            {
                throw new Exception("DeviceService > CopyDevice: An error occurred while coping the device", ex);
            }

        }

        // deze methode is voor om de displayname van de enum te gebruiken
        public string GetDisplayName(Enum value)
        {
            var field = value.GetType().GetField(value.ToString()); // geeft de naam van de enum
            var attribute = (DisplayAttribute)field.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault(); // geeft de displayname van de enum
            return attribute?.Name ?? value.ToString(); // als de displayname leeg is, geef de naam van de enum
        }

    }
}
