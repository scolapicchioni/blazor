export function showMap(lat, lon, msg) {
    const map = L.map('map').setView([lat, lon], 13);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(map);

    L.marker([lat, lon]).addTo(map)
        .bindPopup(msg)
        .openPopup();
}

export function extractCoords(dotNetHelper, img) {
    console.log(`extractCoords invoked with: ${img}`);
    EXIF.getData(img, function () {
        console.log(`innerfunction invoked with: ${this}`);
        const myData = this;

        console.log(myData.exifdata);

        let allMetaData = EXIF.getAllTags(this);
        console.log(JSON.stringify(allMetaData, null, "\t"));

        let { latitude, longitude } = latlon(myData);
        console.log(`extractCoords has gotten the values lat: ${latitude} and lon: ${longitude}`);
        dotNetHelper.invokeMethodAsync('GetLatitudeLongitude', latitude, longitude);
    });
}

function ConvertDMSToDD(degrees, minutes, seconds, direction) {
    //DD = d + (min / 60) + (sec / 3600)
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
    const latDirection = myData.exifdata.GPSLatitudeRef ?? "N";

    const latitude = ConvertDMSToDD(latDegree, latMinute, latSecond, latDirection);
    console.log("Latitude", latitude);

    // Calculate longitude decimal
    const lonDegree = myData.exifdata.GPSLongitude?.[0] ?? 0;
    const lonMinute = myData.exifdata.GPSLongitude?.[1] ?? 0;
    const lonSecond = myData.exifdata.GPSLongitude?.[2] ?? 0;
    const lonDirection = myData.exifdata.GPSLongitudeRef ?? "E";

    const longitude = ConvertDMSToDD(lonDegree, lonMinute, lonSecond, lonDirection);
    console.log("Longitude", longitude);
    return { latitude, longitude };
}