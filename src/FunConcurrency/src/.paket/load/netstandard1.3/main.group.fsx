namespace PaketLoadScripts

#r "../../../packages/NuGet.Common/lib/netstandard1.3/NuGet.Common.dll" 
#r "../../../packages/Microsoft.Extensions.CommandLineUtils/lib/netstandard1.3/Microsoft.Extensions.CommandLineUtils.dll" 
#r "../../../packages/NuGet.Frameworks/lib/netstandard1.3/NuGet.Frameworks.dll" 
#r "../../../packages/NuGet.Versioning/lib/netstandard1.3/NuGet.Versioning.dll" 
#r "../../../packages/System.ValueTuple/lib/netstandard1.0/System.ValueTuple.dll" 
#r "../../../packages/Mono.Cecil/lib/netstandard1.3/Mono.Cecil.dll" 
#r "../../../packages/System.Security.Cryptography.Cng/lib/netstandard1.3/System.Security.Cryptography.Cng.dll" 
#r "../../../packages/System.Xml.XPath.XmlDocument/lib/netstandard1.3/System.Xml.XPath.XmlDocument.dll" 
#r "../../../packages/System.ComponentModel.TypeConverter/lib/netstandard1.0/System.ComponentModel.TypeConverter.dll" 
#r "../../../packages/System.Linq.Queryable/lib/netstandard1.3/System.Linq.Queryable.dll" 
#r "../../../packages/System.Xml.XDocument/lib/netstandard1.3/System.Xml.XDocument.dll" 
#r "../../../packages/System.Xml.XmlDocument/lib/netstandard1.3/System.Xml.XmlDocument.dll" 
#r "../../../packages/System.Xml.XPath/lib/netstandard1.3/System.Xml.XPath.dll" 
#r "../../../packages/Microsoft.DotNet.PlatformAbstractions/lib/netstandard1.3/Microsoft.DotNet.PlatformAbstractions.dll" 
#r "../../../packages/System.Collections.Specialized/lib/netstandard1.3/System.Collections.Specialized.dll" 
#r "../../../packages/System.Diagnostics.TextWriterTraceListener/lib/netstandard1.3/System.Diagnostics.TextWriterTraceListener.dll" 
#r "../../../packages/System.IO.Compression.ZipFile/lib/netstandard1.3/System.IO.Compression.ZipFile.dll" 
#r "../../../packages/System.Threading.Tasks.Parallel/lib/netstandard1.3/System.Threading.Tasks.Parallel.dll" 
#r "../../../packages/System.Xml.ReaderWriter/lib/netstandard1.3/System.Xml.ReaderWriter.dll" 
#r "../../../packages/System.Collections.Concurrent/lib/netstandard1.3/System.Collections.Concurrent.dll" 
#r "../../../packages/System.Collections.NonGeneric/lib/netstandard1.3/System.Collections.NonGeneric.dll" 
#r "../../../packages/System.ComponentModel.EventBasedAsync/lib/netstandard1.3/System.ComponentModel.EventBasedAsync.dll" 
#r "../../../packages/System.Net.WebHeaderCollection/lib/netstandard1.3/System.Net.WebHeaderCollection.dll" 
#r "../../../packages/System.ObjectModel/lib/netstandard1.3/System.ObjectModel.dll" 
#r "../../../packages/System.Reflection.Emit/lib/netstandard1.3/System.Reflection.Emit.dll" 
#r "../../../packages/System.Reflection.Emit.Lightweight/lib/netstandard1.3/System.Reflection.Emit.Lightweight.dll" 
#r "../../../packages/System.Reflection.TypeExtensions/lib/netstandard1.3/System.Reflection.TypeExtensions.dll" 
#r "../../../packages/System.Runtime.Numerics/lib/netstandard1.3/System.Runtime.Numerics.dll" 
#r "../../../packages/System.Runtime.Serialization.Primitives/lib/netstandard1.3/System.Runtime.Serialization.Primitives.dll" 
#r "../../../packages/System.Security.Cryptography.Primitives/lib/netstandard1.3/System.Security.Cryptography.Primitives.dll" 
#r "../../../packages/System.Diagnostics.DiagnosticSource/lib/netstandard1.3/System.Diagnostics.DiagnosticSource.dll" 
#r "../../../packages/System.Reflection.Emit.ILGeneration/lib/netstandard1.3/System.Reflection.Emit.ILGeneration.dll" 
#r "../../../packages/Microsoft.Win32.Registry/lib/netstandard1.3/Microsoft.Win32.Registry.dll" 
#r "../../../packages/System.Threading/lib/netstandard1.3/System.Threading.dll" 
#r "../../../packages/System.Threading.ThreadPool/lib/netstandard1.3/System.Threading.ThreadPool.dll" 
#r "../../../packages/System.IO.FileSystem.Primitives/lib/netstandard1.3/System.IO.FileSystem.Primitives.dll" 
#r "../../../packages/System.Runtime.InteropServices.WindowsRuntime/lib/netstandard1.3/System.Runtime.InteropServices.WindowsRuntime.dll" 
#r "../../../packages/System.Security.AccessControl/lib/netstandard1.3/System.Security.AccessControl.dll" 
#r "../../../packages/System.Threading.Thread/lib/netstandard1.3/System.Threading.Thread.dll" 
#r "../../../packages/System.Memory/lib/netstandard1.1/System.Memory.dll" 
#r "../../../packages/System.Security.Principal.Windows/lib/netstandard1.3/System.Security.Principal.Windows.dll" 
#r "../../../packages/System.Text.Encoding.CodePages/lib/netstandard1.3/System.Text.Encoding.CodePages.dll" 
#r "../../../packages/Microsoft.CodeCoverage/lib/netstandard1.0/Microsoft.VisualStudio.CodeCoverage.Shim.dll" 
#r "../../../packages/System.Buffers/lib/netstandard1.1/System.Buffers.dll" 
#r "../../../packages/System.Collections.Immutable/lib/netstandard1.3/System.Collections.Immutable.dll" 
#r "../../../packages/System.ComponentModel/lib/netstandard1.3/System.ComponentModel.dll" 
#r "../../../packages/System.ComponentModel.Primitives/lib/netstandard1.0/System.ComponentModel.Primitives.dll" 
#r "../../../packages/System.Runtime.CompilerServices.Unsafe/lib/netstandard1.0/System.Runtime.CompilerServices.Unsafe.dll" 
#r "../../../packages/System.Runtime.InteropServices.RuntimeInformation/lib/netstandard1.1/System.Runtime.InteropServices.RuntimeInformation.dll" 
#r "../../../packages/System.Security.Cryptography.ProtectedData/lib/netstandard1.3/System.Security.Cryptography.ProtectedData.dll" 
#r "../../../packages/System.Threading.Tasks.Dataflow/lib/netstandard1.1/System.Threading.Tasks.Dataflow.dll" 
#r "../../../packages/Microsoft.Extensions.DependencyModel/lib/netstandard1.3/Microsoft.Extensions.DependencyModel.dll" 
#r "../../../packages/NuGet.Packaging.Core/lib/netstandard1.3/NuGet.Packaging.Core.dll" 
#r "../../../packages/Newtonsoft.Json/lib/netstandard1.3/Newtonsoft.Json.dll" 
#r "../../../packages/HtmlAgilityPack/lib/netstandard1.3/HtmlAgilityPack.dll" 
#r "../../../packages/Microsoft.CSharp/lib/netstandard1.3/Microsoft.CSharp.dll" 
#r "../../../packages/System.Reflection.Metadata/lib/netstandard1.1/System.Reflection.Metadata.dll" 
#r "../../../packages/Mono.Cecil/lib/netstandard1.3/Mono.Cecil.Rocks.dll" 
#r "../../../packages/Mono.Cecil/lib/netstandard1.3/Mono.Cecil.Pdb.dll" 
#r "../../../packages/Mono.Cecil/lib/netstandard1.3/Mono.Cecil.Mdb.dll" 
#r "../../../packages/System.Private.DataContractSerialization/lib/netstandard1.3/System.Private.DataContractSerialization.dll" 
#r "../../../packages/System.Xml.XmlSerializer/lib/netstandard1.3/System.Xml.XmlSerializer.dll" 
#r "../../../packages/SixLabors.Core/lib/netstandard1.1/SixLabors.Core.dll" 
#r "../../../packages/System.Threading.Tasks.Extensions/lib/netstandard1.0/System.Threading.Tasks.Extensions.dll" 
#r "../../../packages/System.Dynamic.Runtime/lib/netstandard1.3/System.Dynamic.Runtime.dll" 
#r "../../../packages/NuGet.Packaging/lib/netstandard1.3/NuGet.Packaging.dll" 
#r "../../../packages/System.Runtime.Serialization.Json/lib/netstandard1.3/System.Runtime.Serialization.Json.dll" 
#r "../../../packages/SixLabors.Fonts/lib/netstandard1.3/SixLabors.Fonts.dll" 
#r "../../../packages/SixLabors.ImageSharp/lib/netstandard1.3/SixLabors.ImageSharp.dll" 
#r "../../../packages/SixLabors.Shapes/lib/netstandard1.1/SixLabors.Shapes.dll" 
#r "../../../packages/SixLabors.ImageSharp.Drawing/lib/netstandard1.1/SixLabors.ImageSharp.Drawing.dll" 
#r "System.Security" 
#r "System.Core" 
#r "System" 
#r "System.ComponentModel.Composition" 
#r "System.Configuration" 
#r "System.Xml" 
#r "System.Runtime.Serialization" 
#r "System.Numerics" 
#r "System.IO.Compression" 
#r "System.IO.Compression.FileSystem" 
#r "System.Xaml" 
#r "System.Xml.Linq" 
#r "System.Net.Http" 
#r "Microsoft.CSharp" 
#r "System.Windows" 
#r "System.Windows.Forms" 
#r "WindowsBase" 
#r "System.Runtime" 
#r "ISymWrapper, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" 
#r "System.IO" 