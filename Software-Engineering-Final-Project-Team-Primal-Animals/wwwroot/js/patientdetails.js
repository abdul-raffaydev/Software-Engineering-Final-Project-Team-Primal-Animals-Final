// ======================
// HEATMAP
// ======================

let reportAvgPeak = 0;
let reportAvgContact = 0;
let reportThreshold = 0;

function drawHeatmap(matrix) {
    const canvas = document.getElementById("heatmapCanvas");
    const ctx = canvas.getContext("2d");

    const rect = canvas.getBoundingClientRect();
    canvas.width = rect.width * window.devicePixelRatio;
    canvas.height = rect.height * window.devicePixelRatio;

    ctx.setTransform(window.devicePixelRatio, 0, 0, window.devicePixelRatio, 0, 0);
    ctx.clearRect(0, 0, rect.width, rect.height);

    if (!matrix || !matrix.length) return;

    const rows = matrix.length;
    const cols = matrix[0].length;

    const cellW = rect.width / cols;
    const cellH = rect.height / rows;

    let maxVal = 1;
    matrix.forEach(r => r.forEach(v => maxVal = Math.max(maxVal, v)));

    function getColor(value) {
        const n = value / maxVal;
        if (n < 0.4) return "rgb(255,200,60)";
        if (n < 0.8) return "rgb(255,120,40)";
        return "rgb(200,50,20)";
    }

    for (let r = 0; r < rows; r++)
        for (let c = 0; c < cols; c++) {
            ctx.fillStyle = getColor(matrix[r][c]);
            ctx.fillRect(c * cellW, r * cellH, cellW, cellH);
        }
}

// ======================
// FRAME LOADER
// ======================
async function loadFrameByIndex(index) {
    const loader = document.getElementById("heatmapLoader");
    loader.classList.add("show");

    try {
        if (frames.length === 0) return;

        index = Math.max(0, Math.min(index, frames.length - 1));
        const frame = frames[index];

        const resp = await fetch(`/Clinician/GetPatientFrame?patientId=${patientId}&dataId=${frame.DataId}`);
        if (!resp.ok) return;

        const json = await resp.json();

        document.getElementById("frameLabel").textContent =
            `Frame: ${new Date(json.timestamp).toLocaleString()}`;
        document.getElementById("commentDataId").value = json.dataId;

        drawHeatmap(json.matrix);
    }
    finally {
        setTimeout(() => loader.classList.remove("show"), 200);
    }
}

// ======================
// BUILD CHARTS
// ======================
let peakChart, contactChart;

function buildCharts(peakSeries, contactSeries) {
    const opts = {
        responsive: true,
        maintainAspectRatio: true,
        aspectRatio: 3,
        plugins: { legend: { display: false } },
        elements: { point: { radius: 0 } }
    };

    console.log("Peak series is:", peakSeries);

    // ===================
    // PEAK LINE CHART
    // ===================
    const peakCtx = document.getElementById("peakChart").getContext("2d");
    if (peakChart) peakChart.destroy();

    peakChart = new Chart(peakCtx, {
        type: "line",
        data: {
            labels: peakSeries.map(x => new Date(x.Time).toLocaleDateString()), // <-- FIXED
            datasets: [{
                label: "Peak Pressure",
                data: peakSeries.map(x => x.Value), // <-- FIXED
                borderWidth: 2,
                borderColor: "blue",
                fill: false,
                tension: 0
            }]
        },
        options: opts
    });

    // ===================
    // CONTACT LINE CHART
    // ===================
    const contactCtx = document.getElementById("contactChart").getContext("2d");
    if (contactChart) contactChart.destroy();

    contactChart = new Chart(contactCtx, {
        type: "line",
        data: {
            labels: contactSeries.map(x => new Date(x.Time).toLocaleDateString()), // <-- FIXED
            datasets: [{
                label: "Contact Area",
                data: contactSeries.map(x => x.Value), // <-- FIXED
                borderWidth: 2,
                borderColor: "green",
                fill: false,
                tension: 0
            }]
        },
        options: opts
    });
}


// ======================
// TIME SERIES LOADER
// ======================
async function loadTimeSeries(period) {
    const resp = await fetch(`/Clinician/GetPatientTimeSeries?patientId=${patientId}&period=${period}`);
    if (!resp.ok) return;

    const data = await resp.json();

    const peak = data.map(x => ({ time: x.time, value: x.peak }));
    const contact = data.map(x => ({ time: x.time, value: x.contact }));

    buildCharts(peak, contact);
}

// ======================
// THRESHOLD SAVE
// ======================
async function saveThreshold(ev) {
    ev.preventDefault();

    const threshold = Number(document.getElementById("thresholdValue").value);
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

    const resp = await fetch("/Clinician/UpdateThreshold", {
        method: "POST",
        headers: { "Content-Type": "application/x-www-form-urlencoded" },
        body: new URLSearchParams({
            patientId,
            threshold,
            __RequestVerificationToken: token
        })
    });

    if (!resp.ok) {
        document.getElementById("thresholdMsg").textContent = "Save failed.";
        return;
    }

    const json = await resp.json();
    document.getElementById("thresholdMsg").textContent =
        `Saved threshold: ${json.highPressureThreshold}`;


}

// ======================
// COMMENT SUBMIT
// ======================
async function submitComment(ev) {
    ev.preventDefault();

    const text = document.getElementById("commentText").value.trim();
    if (!text) return alert("Enter a comment.");

    const dataId = Number(document.getElementById("commentDataId").value);
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

    const resp = await fetch("/Clinician/PostComment", {
        method: "POST",
        headers: { "Content-Type": "application/x-www-form-urlencoded" },
        body: new URLSearchParams({
            patientId,
            dataId,
            content: text,
            __RequestVerificationToken: token
        })
    });

    if (!resp.ok) return;

    const json = await resp.json();
    const content = json.Content ?? json.content ?? text;
    const time = json.CommentTime ?? json.commentTime ?? new Date();

    const bubble = document.createElement("div");
    bubble.className = "comment-bubble";
    bubble.innerHTML = `
        <div class="small text-muted">You (${new Date(time).toLocaleString()})</div>
        <div>${content}</div>
    `;

    document.getElementById("commentsList").prepend(bubble);
    document.getElementById("commentText").value = "";
}

// ======================
// SIMPLE REPORT
// ======================
function safeAvg(arr, key) {
    if (!arr || arr.length === 0) return 0;
    return arr.reduce((sum, x) => sum + (x[key] || 0), 0) / arr.length;
}

async function generateReport() {

    const now = new Date();

    const fromDate = new Date(now.getTime() - 24 * 3600 * 1000);
    const toDate = now;

    const url = new URL("/Clinician/GeneratePdfReport", window.location.origin);
    url.searchParams.append("patientId", patientId);
    url.searchParams.append("from", fromDate.toISOString());
    url.searchParams.append("to", toDate.toISOString());

    // Force download
    window.location.href = url.toString();
}



// ======================
// PAGE INITIALIZATION
// ======================
function initPatientDetailsPage() {
    drawHeatmap(initialHeatmap);
    buildCharts(peakSeries, contactSeries);

    document.getElementById("frameRange").addEventListener("input", e => {
        currentFrameIndex = Number(e.target.value);
        loadFrameByIndex(currentFrameIndex);
    });

    document.getElementById("btn1h").onclick = () => loadTimeSeries("1h");
    document.getElementById("btn6h").onclick = () => loadTimeSeries("6h");
    document.getElementById("btn24h").onclick = () => loadTimeSeries("24h");
    document.getElementById("btn7d").onclick = () => loadTimeSeries("7d");

    document.getElementById("commentForm").addEventListener("submit", submitComment);
    document.getElementById("thresholdForm").addEventListener("submit", saveThreshold);
    document.getElementById("btnReport").onclick = generateReport;
}
