// This is a JavaScript module that is loaded on demand. It can export any number of
// functions, and may import other JavaScript modules if required.

export function showMap(lat, lon, msg) {
    const map = L.map('map').setView([lat, lon], 13);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(map);

    L.marker([lat, lon]).addTo(map)
        .bindPopup(msg)
        .openPopup();
}

export function extractCoords(img) {
    EXIF.getData(img, function () {
        latlon(this);
    });
}

function ConvertDMSToDD(degrees, minutes, seconds, direction) {
    let dd = degrees + (minutes / 60) + (seconds / 3600);

    if (direction == "S" || direction == "W") {
        dd = dd * -1;
    }

    return dd;
}

function latlon(myData) {
    // Calculate latitude decimal
    const latDegree = myData.exifdata.GPSLatitude?.[0] ?? 0;
    const latMinute = myData.exifdata.GPSLatitude?.[1] ?? 0;
    const latSecond = myData.exifdata.GPSLatitude?.[2] ?? 0;
    const latDirection = myData.exifdata.GPSLatitudeRef ?? 'N';
    
    const latFinal = ConvertDMSToDD(latDegree, latMinute, latSecond, latDirection);

    // Calculate longitude decimal
    const lonDegree = myData.exifdata.GPSLongitude?.[0] ?? 0;
    const lonMinute = myData.exifdata.GPSLongitude?.[1] ?? 0;
    const lonSecond = myData.exifdata.GPSLongitude?.[2] ?? 0;
    const lonDirection = myData.exifdata.GPSLongitudeRef ?? 'E';
    
    const lonFinal = ConvertDMSToDD(lonDegree, lonMinute, lonSecond, lonDirection);

    DotNet.invokeMethodAsync('PhotoSharingApplication.Frontend.BlazorComponents', 'UpdatePhotoCoordinates', latFinal, lonFinal);
}
