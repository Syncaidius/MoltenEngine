using Silk.NET.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>
    /// See: https://docs.microsoft.com/en-us/windows/win32/direct3ddxgi/dxgi-error
    /// </summary>
    public enum DxgiError : uint
    {
        /// <summary>
        /// The method succeeded without an error.
        /// </summary>
        Ok = 0,

        /// <summary>
        /// You tried to use a resource to which you did not have the required access privileges. 
        /// This error is most typically caused when you write to a shared resource with read-only access.
        /// </summary>
        AcessDenied = 0x887A002B,

        /// <summary>
        /// The desktop duplication interface is invalid.
        /// The desktop duplication interface typically becomes invalid when a different type of image is displayed on the desktop.
        /// </summary>
        AccessLost = 0x887A0026,

        /// <summary>
        /// The desired element already exists.
        /// This is returned by DXGIDeclareAdapterRemovalSupport if it is not the first time that the function is called.
        /// </summary>
        AlreadyExists = 0x887A0036,

        /// <summary>
        /// DXGI can't provide content protection on the swap chain. This error is typically caused by an older driver, or when you use a swap chain that is incompatible with content protection.
        /// </summary>
        CannotProtectContent = 0x887A002A,

        /// <summary>
        /// The application's device failed due to badly formed commands sent by the application. 
        /// This is an design-time issue that should be investigated and fixed.
        /// </summary>
        DeviceHung = 0x887A0006,

        /// <summary>
        /// The video card has been physically removed from the system, or a driver upgrade for the video
        /// card has occurred.The application should destroy and recreate the device. For help debugging the problem, 
        /// call ID3D10Device::GetDeviceRemovedReason.
        /// </summary>
        DeviceRemoved = 0x887A0005,

        /// <summary>
        /// The device failed due to a badly formed command.This is a run-time issue; 
        /// The application should destroy and recreate the device.
        /// </summary>
        DeviceReset = 0x887A0007,

        /// <summary>
        /// The driver encountered a problem and was put into the device removed state.
        /// </summary>
        DriverInternalError = 0x887A0020,

        /// <summary>
        /// An event (for example, a power cycle) interrupted the gathering of presentation statistics.
        /// </summary>
        FrameStatisticDisjoint = 0x887A000B,

        /// <summary>
        /// The application attempted to acquire exclusive ownership of an output, 
        /// but failed because some other application (or device within the application) already acquired ownership.
        /// </summary>
        GraphicsVidpnSourceInUse = 0x887A000C,

        /// <summary>
        /// The application provided invalid parameter data; this must be debugged and fixed before the application is released.
        /// </summary>
        InvalidCall = 0x887A0001,

        /// <summary>
        /// The buffer supplied by the application is not big enough to hold the requested data.
        /// </summary>
        MoreData = 0x887A0003,

        /// <summary>
        /// The supplied name of a resource in a call to IDXGIResource1::CreateSharedHandle is already associated with some other resource.
        /// </summary>
        NameAlreadyExists = 0x887A002C,

        /// <summary>
        /// A global counter resource is in use, and the Direct3D device can't currently use the counter resource.
        /// </summary>
        NonExclusive = 0x887A0021,

        /// <summary>
        /// The resource or request is not currently available, but it might become available later.
        /// </summary>
        NotCurrentlyAvailable = 0x887A0022,

        /// <summary>
        /// When calling IDXGIObject::GetPrivateData, the GUID passed in is not recognized as one previously passed to IDXGIObject::SetPrivateData or IDXGIObject::SetPrivateDataInterface.When calling IDXGIFactory::EnumAdapters or IDXGIAdapter::EnumOutputs, the enumerated ordinal is out of range.
        /// </summary>
        NotFound = 0x887A0002,

        /// <summary>
        /// Reserved
        /// </summary>
        RemoteClientDisconnected = 0x887A0023,

        /// <summary>
        /// Reserved
        /// </summary>
        RemoteOutOfMemory = 0x887A0024,

        /// <summary>
        /// The DXGI output (monitor) to which the swap chain content was restricted is now disconnected or changed.
        /// </summary>
        RestrictToOutputStale = 0x887A0029,

        /// <summary>
        /// The operation depends on an SDK component that is missing or mismatched.
        /// </summary>
        SdkComponentMissing = 0x887A002D,

        /// <summary>
        /// The Remote Desktop Services session is currently disconnected.
        /// </summary>
        SessionDisconnected = 0x887A0028,

        /// <summary>
        /// The requested functionality is not supported by the device or the driver.
        /// </summary>
        Unsupported = 0x887A0004,

        /// <summary>
        /// The time-out interval elapsed before the next desktop frame was available.
        /// </summary>
        WaitTimeout = 0x887A0027,

        /// <summary>
        /// The GPU was busy at the moment when a call was made to perform an operation, and did not execute or schedule the operation.
        /// </summary>
        WasStillDrawing = 0x887A000A,
    }
}
