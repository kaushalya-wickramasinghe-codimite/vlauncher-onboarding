// Background service worker for VLauncher Chrome Extension
console.log('VLauncher background service worker started');

// Listen for installation
chrome.runtime.onInstalled.addListener(() => {
  console.log('VLauncher extension installed');
});
