using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.X509;
using PKISharp.WACS.Services;
using System.Collections.Generic;
using System.IO;

namespace PKISharp.WACS.DomainObjects
{
    /// <summary>
    /// Special implementation of CertificateInfo which contains reference
    /// to a file in the cache
    /// </summary>
    public class CertificateInfoCache : ICertificateInfo
    {
        private readonly ICertificateInfo _inner;

        /// <summary>
        /// Initializes a new instance of the <see cref="CertificateInfoCache"/> class
        /// by loading certificate information from the specified cached PFX file.
        /// </summary>
        /// <param name="file">The cached certificate file on disk.</param>
        /// <param name="password">The password used to decrypt the PFX file, or <c>null</c> if none.</param>
        public CertificateInfoCache(FileInfo file, string? password)  
        {
            CacheFile = file;

            using var stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
            var wrapper = PfxService.GetPfx(PfxProtectionMode.Default);
            wrapper.Store.Load(stream, password?.ToCharArray());
            _inner = new CertificateInfo(wrapper);
        }

        /// <summary>
        /// Location on disk
        /// </summary>
        public FileInfo CacheFile { get; private set; }

        public X509Certificate Certificate => _inner.Certificate;
        public IEnumerable<X509Certificate> Chain => _inner.Chain;
        public PfxWrapper Collection => _inner.Collection;
        public Identifier? CommonName => _inner.CommonName;
        public AsymmetricKeyParameter? PrivateKey => _inner.PrivateKey;
        public IEnumerable<Identifier> SanNames => _inner.SanNames;
        public string FriendlyName => _inner.FriendlyName;
        public string Thumbprint => _inner.Thumbprint;
        public byte[] GetHash() => _inner.GetHash();
    }
}
