import { identityService } from './services/identity.service';
import { apiService } from './services/api.service';

interface UIElements {
  statusDiv: HTMLElement;
  emailDiv: HTMLElement;
  registerBtn: HTMLButtonElement;
  messageDiv: HTMLElement;
}

class PopupController {
  private elements: UIElements;
  private userEmail: string | null = null;

  constructor() {
    this.elements = {
      statusDiv: document.getElementById('status') as HTMLElement,
      emailDiv: document.getElementById('email') as HTMLElement,
      registerBtn: document.getElementById('registerBtn') as HTMLButtonElement,
      messageDiv: document.getElementById('message') as HTMLElement,
    };

    this.init();
  }

  private async init(): Promise<void> {
    this.showLoading();

    const userInfo = await identityService.getUserEmail();

    if (userInfo && userInfo.email) {
      this.userEmail = userInfo.email;
      this.showUserInfo(userInfo.email);
    } else {
      this.showNotSignedIn();
    }

    this.elements.registerBtn.addEventListener('click', () => this.handleRegister());
  }

  private showLoading(): void {
    this.elements.statusDiv.textContent = 'Loading...';
    this.elements.emailDiv.textContent = '';
    this.elements.registerBtn.style.display = 'none';
    this.elements.messageDiv.textContent = '';
  }

  private showUserInfo(email: string): void {
    this.elements.statusDiv.textContent = 'Signed in as:';
    this.elements.emailDiv.textContent = email;
    this.elements.registerBtn.style.display = 'block';
    this.elements.registerBtn.disabled = false;
  }

  private showNotSignedIn(): void {
    this.elements.statusDiv.textContent = 'Not signed in to Chrome';
    this.elements.emailDiv.textContent = 'Please sign in to Chrome to use this extension';
    this.elements.registerBtn.style.display = 'none';
  }

  private async handleRegister(): Promise<void> {
    if (!this.userEmail) {
      this.showMessage('No email available', 'error');
      return;
    }

    this.elements.registerBtn.disabled = true;
    this.elements.registerBtn.textContent = 'Registering...';
    this.showMessage('', '');

    const result = await apiService.registerUser(this.userEmail);

    if (result.success) {
      this.showMessage('Registration successful!', 'success');
      this.elements.registerBtn.textContent = 'Registered';
    } else {
      this.showMessage(result.error || 'Registration failed', 'error');
      this.elements.registerBtn.disabled = false;
      this.elements.registerBtn.textContent = 'Register';
    }
  }

  private showMessage(text: string, type: 'success' | 'error' | ''): void {
    this.elements.messageDiv.textContent = text;
    this.elements.messageDiv.className = `message ${type}`;
  }
}

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
  new PopupController();
});
