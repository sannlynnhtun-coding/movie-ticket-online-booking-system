// Cinematix IndexedDB module
// Manages purchased ticket persistence in the browser

const DB_NAME = 'CinematixDb';
const DB_VERSION = 1;
const STORE_TICKETS = 'tickets';

let _db = null;

function openDb() {
    return new Promise((resolve, reject) => {
        if (_db) { resolve(_db); return; }
        const req = indexedDB.open(DB_NAME, DB_VERSION);
        req.onupgradeneeded = (e) => {
            const db = e.target.result;
            if (!db.objectStoreNames.contains(STORE_TICKETS)) {
                const store = db.createObjectStore(STORE_TICKETS, { keyPath: 'bookingVoucherDetailId' });
                store.createIndex('bookingDate', 'bookingDate', { unique: false });
            }
        };
        req.onsuccess = (e) => { _db = e.target.result; resolve(_db); };
        req.onerror = (e) => reject(e.target.error);
    });
}

window.cinematix = {
    initDb: async function () {
        await openDb();
        return true;
    },

    saveTickets: async function (tickets) {
        const db = await openDb();
        return new Promise((resolve, reject) => {
            const tx = db.transaction(STORE_TICKETS, 'readwrite');
            const store = tx.objectStore(STORE_TICKETS);
            let count = 0;
            for (const ticket of tickets) {
                store.put(ticket);
                count++;
            }
            tx.oncomplete = () => resolve(count);
            tx.onerror = (e) => reject(e.target.error);
        });
    },

    getAllTickets: async function () {
        const db = await openDb();
        return new Promise((resolve, reject) => {
            const tx = db.transaction(STORE_TICKETS, 'readonly');
            const store = tx.objectStore(STORE_TICKETS);
            const req = store.getAll();
            req.onsuccess = () => resolve(req.result);
            req.onerror = (e) => reject(e.target.error);
        });
    },

    clearTickets: async function () {
        const db = await openDb();
        return new Promise((resolve, reject) => {
            const tx = db.transaction(STORE_TICKETS, 'readwrite');
            const store = tx.objectStore(STORE_TICKETS);
            const req = store.clear();
            req.onsuccess = () => resolve(true);
            req.onerror = (e) => reject(e.target.error);
        });
    },

    deleteTicket: async function (id) {
        const db = await openDb();
        return new Promise((resolve, reject) => {
            const tx = db.transaction(STORE_TICKETS, 'readwrite');
            const store = tx.objectStore(STORE_TICKETS);
            const req = store.delete(id);
            req.onsuccess = () => resolve(true);
            req.onerror = (e) => reject(e.target.error);
        });
    },

    // Theme helpers
    getTheme: function () {
        return localStorage.getItem('cx-theme') || 'dark';
    },
    setTheme: function (theme) {
        localStorage.setItem('cx-theme', theme);
        if (theme === 'dark') {
            document.documentElement.classList.add('dark');
        } else {
            document.documentElement.classList.remove('dark');
        }
        return theme;
    },
    toggleTheme: function () {
        const current = window.cinematix.getTheme();
        return window.cinematix.setTheme(current === 'dark' ? 'light' : 'dark');
    }
};

// Apply saved theme on load immediately
(function () {
    const saved = localStorage.getItem('cx-theme') || 'dark';
    if (saved === 'dark') document.documentElement.classList.add('dark');
})();
