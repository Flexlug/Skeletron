
#FROM debian:latest AS cmake
#
#ARG CMAKE_VERSION="3.15.2"
#
#RUN apt-get update \
    #&& apt-get install -y  \
        #build-essential \
        #autoconf \
        #automake \
        #libtool \
        #libtiff5-dev \
        #zlib1g-dev \
        #libpango1.0-dev \
        #wget \
    #&& rm -rf /var/lib/apt/lists/*
#
#WORKDIR /install
#
#RUN wget https://github.com/Kitware/CMake/releases/download/v$CMAKE_VERSION/cmake-$CMAKE_VERSION.tar.gz \
    #&& tar -zxvf cmake-$CMAKE_VERSION.tar.gz \
    #&& cd cmake-3.15.2 \
    #&& ./bootstrap \
    #&& make \
    #&& make install \
    #&& rm ../cmake-$CMAKE_VERSION.tar.gz
    #
#WORKDIR /build
#
#FROM cmake AS leptonica_builder
#
#ARG LEPTONICA_VERSION="1.80.0"
#
#RUN wget http://www.leptonica.org/source/leptonica-$LEPTONICA_VERSION.tar.gz \
    #&& tar -xvf leptonica-$LEPTONICA_VERSION.tar.gz \
    #&& cd leptonica-$LEPTONICA_VERSION \
    #&& mkdir build && cd build \
    #&& cmake .. -DBUILD_SHARED_LIBS=ON \
    #&& make \
    #&& make install \
    #&& cd src \
    #&& cp libleptonica.so.$LEPTONICA_VERSION ../../../libleptonica-$LEPTONICA_VERSION.so
## /build/libleptonica-$LEPTONICA_VERSION.so
#
## Leptonica is a dependency of tesseract
#FROM leptonica_builder AS tesseract_builder
#
#ARG TESSERACT_VERSION="4.1.0"
#ARG TESSERACT_FILENAME="libtesseract41.so"
#
#RUN wget https://github.com/tesseract-ocr/tesseract/archive/refs/tags/$TESSERACT_VERSION.tar.gz \
    #&& tar -xvf $TESSERACT_VERSION.tar.gz \
    #&& cd tesseract-$TESSERACT_VERSION \
    #&& mkdir build && cd build \
    #&& cmake .. -DBUILD_SHARED_LIBS=ON \
    #&& make \
    #&& make install \
    #&& cp libtesseract.so.$TESSERACT_VERSION ../../$TESSERACT_FILENAME
## /build/$TESSERACT_FILENAME

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:6.0 AS builder

WORKDIR /src

COPY ["Skeletron/Skeletron.csproj", "Skeletron/"]
COPY ["Osu.NET.Api/Osu.NET.Api.csproj", "Osu.NET.Api/"]
COPY ["Osu.NET.Recognizer/Osu.NET.Recognizer.csproj", "Osu.NET.Recognizer/"]

RUN dotnet restore "Skeletron/Skeletron.csproj"

COPY . /src
WORKDIR /src/Skeletron
RUN dotnet publish "Skeletron.csproj" -c Release -o /app/publish

#FROM debian:latest AS library_composer
#
#ARG LEPTONICA_VERSION="1.80.0"
#ARG TESSERACT_VERSION="4.1.0"
#ARG TESSERACT_FILENAME="libtesseract41.so"
#
#WORKDIR /compose
#RUN mkdir libs
#COPY --from=leptonica_builder /build/libleptonica-$LEPTONICA_VERSION.so libs
#COPY --from=tesseract_builder /build/$TESSERACT_FILENAME libs
#
#ARG TARGETPLATFORM
#
#RUN case ${TARGETPLATFORM} in \
         #"linux/amd64")  ARCH="x64"  ;; \
         #"linux/arm64")  ARCH="x64"  ;; \
         #"linux/arm/v7") ARCH="x86"  ;; \
         #"linux/arm/v6") ARCH="x86"  ;; \
    #esac \
    #&& mv libs $ARCH

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runner

#RUN dpkg --add-architecture i386
#RUN apt-get update
#RUN apt-get install libc6-dev -y

WORKDIR /app
COPY --from=builder /app/publish ./
#COPY --from=library_composer /compose ./

VOLUME ["/app/data"]

ENTRYPOINT ["dotnet", "Skeletron.dll"]
