function setupAutoRefresh() {
    let intervalId = null;

    const checkbox = document.getElementById("autoRefreshSwitch");
    if (!checkbox) return;

    checkbox.addEventListener("change", function () {
        if (this.checked) {
            intervalId = setInterval(() => {
                location.reload();
            }, 5000);
        } else {
            clearInterval(intervalId);
        }
    });
}

// Call this function when the page loads
document.addEventListener("DOMContentLoaded", setupAutoRefresh);
