@rem Copyright 2016 gRPC authors.
@rem
@rem Licensed under the Apache License, Version 2.0 (the "License");
@rem you may not use this file except in compliance with the License.
@rem You may obtain a copy of the License at
@rem
@rem     http://www.apache.org/licenses/LICENSE-2.0
@rem
@rem Unless required by applicable law or agreed to in writing, software
@rem distributed under the License is distributed on an "AS IS" BASIS,
@rem WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
@rem See the License for the specific language governing permissions and
@rem limitations under the License.

@rem Generate the C# code for .proto files

@rem Run from Istio deployment: src\istio.io\istio\mixer\template\tracespan

setlocal

@rem enter this directory
cd /d %~dp0

@rem CHANGE THIS TO YOUR LOCAL ENLISTMENT OF https://github.com/census-instrumentation/opencensus-proto
set PROTODIR="E:\AKS\MixerAdapter\src\istio.io\istio\mixer\template\tracespan"

@rem CHANGE THIS TO THE APPROPRIATE VERSION OF Google.Protobuf.Tools NuGet package present on your machine
set PROTOCDIR=%UserProfile%\.nuget\packages\Google.Protobuf.Tools\3.6.0\tools\
set PROTOC=%PROTOCDIR%\windows_x64\protoc.exe

set ISTIOPROTOROOT=E:\AKS\MixerAdapter\src\istio.io\istio\vendor\istio.io\api\

@rem CHANGE THIS TO THE APPROPRIATE VERSION OF Grpc.Tools NuGet package present on your machine
set PLUGIN=%UserProfile%\.nuget\packages\Grpc.Tools\1.13.1\tools\windows_x64\grpc_csharp_plugin.exe

@rem @echo Generating protobuf messages...
@rem %PROTOC% -I=%ISTIOPROTOROOT% --proto_path=%PROTOCDIR% --proto_path=%PROTODIR% --csharp_out=.\code --csharp_opt=file_extension=.g.cs  %PROTODIR%\tracespan.proto 

@echo Generating GRPC services...
%PROTOC% -I=%ISTIOPROTOROOT% --proto_path=%ISTIOPROTOROOT%\..\..\ --proto_path=%PROTOCDIR% --proto_path=%PROTODIR% --proto_path=E:\AKS\MixerAdapter\src\istio.io\istio\vendor\github.com\gogo\protobuf\ --csharp_out=.\code --csharp_opt=file_extension=.g.cs --grpc_out=.\code --plugin=protoc-gen-grpc=%PLUGIN% %PROTODIR%\tracespan_handler_service.proto %PROTODIR%\tracespan.proto %ISTIOPROTOROOT%\policy\v1beta1\type.proto %ISTIOPROTOROOT%\policy\v1beta1\value_type.proto %ISTIOPROTOROOT%\mixer\adapter\model\v1beta1\extensions.proto %ISTIOPROTOROOT%\mixer\adapter\model\v1beta1\report.proto 
@rem %ISTIOPROTOROOT%\..\..\github.com\gogo\protobuf\gogoproto\gogo.proto

endlocal
