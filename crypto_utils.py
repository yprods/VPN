"""
Encryption utilities for VPN communication
"""
from cryptography.fernet import Fernet
from cryptography.hazmat.primitives import hashes
from cryptography.hazmat.primitives.kdf.pbkdf2 import PBKDF2HMAC
from cryptography.hazmat.backends import default_backend
import base64
import os


class VPNCrypto:
    """Handles encryption and decryption for VPN traffic"""
    
    def __init__(self, password: str = None):
        """
        Initialize encryption with a password
        
        Args:
            password: Password for key derivation. If None, generates a random key.
        """
        if password:
            self.key = self._derive_key(password)
        else:
            # Generate a random key
            self.key = Fernet.generate_key()
        
        self.cipher = Fernet(self.key)
    
    def _derive_key(self, password: str) -> bytes:
        """Derive encryption key from password"""
        password_bytes = password.encode()
        salt = b'vpn_salt_12345'  # In production, use random salt
        
        kdf = PBKDF2HMAC(
            algorithm=hashes.SHA256(),
            length=32,
            salt=salt,
            iterations=100000,
            backend=default_backend()
        )
        key = base64.urlsafe_b64encode(kdf.derive(password_bytes))
        return key
    
    def encrypt(self, data: bytes) -> bytes:
        """Encrypt data"""
        return self.cipher.encrypt(data)
    
    def decrypt(self, encrypted_data: bytes) -> bytes:
        """Decrypt data"""
        return self.cipher.decrypt(encrypted_data)
    
    def get_key(self) -> bytes:
        """Get the encryption key (for sharing with client)"""
        return self.key
    
    def set_key(self, key: bytes):
        """Set encryption key from bytes"""
        self.key = key
        self.cipher = Fernet(key)

