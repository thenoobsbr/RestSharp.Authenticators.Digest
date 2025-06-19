﻿using System;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace RestSharp.Authenticators.Digest;

internal class DigestHeader
{
    public const string NONCE = "nonce";

    public const int NONCE_COUNT = 1;

    public const string QOP = "qop";

    public const string REALM = "realm";

    public const string OPAQUE = "opaque";

    public const string REGEX_PATTERN =
        "realm=\"(?<realm>.*?)\"|qop=(?:\"(?<qop>.*?)\"|(?<qop>[^\",\\s]+))|nonce=\"(?<nonce>.*?)\"|stale=\"(?<stale>.*?)\"|opaque=\"(?<opaque>.*?)\"|domain=\"(?<domain>.*?)\"";
    
    private static readonly Regex _regex;

    static DigestHeader()
    {
        _regex = new Regex(
            REGEX_PATTERN,
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }

    public DigestHeader(string authenticateHeader, ILogger logger)
    {
        var matches = _regex.Matches(authenticateHeader);
        foreach (Match m in matches)
        {
            if (!m.Success)
            {
                continue;
            }

            if (m.Groups[QOP].Success)
            {
                Qop = m.Groups[QOP].Value;
            }

            if (m.Groups[REALM].Success)
            {
                Realm = m.Groups[REALM].Value;
            }

            if (m.Groups[NONCE].Success)
            {
                Nonce = m.Groups[NONCE].Value;
            }

            if (m.Groups[OPAQUE].Success)
            {
                Opaque = m.Groups[OPAQUE].Value;
            }
        }

        if (AllDataCorrectFilled())
        {
            return;
        }

        logger.LogError("Cannot load all required data from {AuthenticateHeaderName}. Data: {AuthenticateHeader}", nameof(authenticateHeader), authenticateHeader);
        throw new ArgumentException(
            $"Cannot load all required data from {nameof(authenticateHeader)}. Data: {authenticateHeader}");
    }

    public string? Nonce { get; }
    public string? Qop { get; }
    public string? Realm { get; }
    public string? Opaque { get; }

    public override string ToString()
    {
        return $"{nameof(Realm)}=\"{Realm}\"&{nameof(Nonce)}=\"{Nonce}\"&{nameof(Qop)}=\"{Qop}\"&{nameof(Opaque)}=\"{Opaque}\"";
    }

    private bool AllDataCorrectFilled()
    {
        return !string.IsNullOrWhiteSpace(Nonce)
               && !string.IsNullOrWhiteSpace(Qop)
               && !string.IsNullOrWhiteSpace(Realm);
    }
}
