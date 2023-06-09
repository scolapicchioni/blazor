export function showMap(lat, lon, msg) {
    const map = L.map('map').setView([lat, lon], 13);

    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(map);

    L.marker([lat, lon]).addTo(map)
        .bindPopup(msg)
        .openPopup();
}