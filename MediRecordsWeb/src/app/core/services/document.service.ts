import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  DocumentDownloadFile,
  DocumentListFilter,
  DocumentListItem,
  DocumentUploadResponse
} from '../models/document.models';

export interface DocumentUploadInput {
  patientId: number;
  encounterId: number;
  docType: string;
  providerOnlyVisibility: boolean;
  file: File;
}

@Injectable({ providedIn: 'root' })
export class DocumentService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/Document`;
  private readonly tz =
    Intl.DateTimeFormat().resolvedOptions().timeZone || 'UTC';

  upload(input: DocumentUploadInput): Observable<DocumentUploadResponse> {
    const form = new FormData();
    form.append('PatientId', String(input.patientId));
    form.append('EncounterId', String(input.encounterId));
    form.append('DocType', input.docType);
    form.append('ProviderOnlyVisibility', String(input.providerOnlyVisibility));
    form.append('File', input.file, input.file.name);

    return this.http.post<DocumentUploadResponse>(`${this.base}/upload`, form);
  }

  search(filter: DocumentListFilter = {}): Observable<DocumentListItem[]> {
    let params = new HttpParams();
    if (filter.patientId != null) params = params.set('patientId', filter.patientId);
    if (filter.encounterId != null) params = params.set('encounterId', filter.encounterId);
    if (filter.uploadedDate) params = params.set('uploadedDate', filter.uploadedDate);

    return this.http.get<DocumentListItem[]>(`${this.base}/search`, {
      params,
      headers: new HttpHeaders({ 'X-Timezone': this.tz })
    });
  }

  download(documentId: number): Observable<DocumentDownloadFile> {
    return this.http
      .get(`${this.base}/download/${documentId}`, {
        responseType: 'blob',
        observe: 'response',
        headers: new HttpHeaders({ 'X-Timezone': this.tz })
      })
      .pipe(
        map((res) => {
          const blob = res.body ?? new Blob();
          const fileName = parseFileName(res.headers.get('content-disposition'))
            ?? `document-${documentId}`;
          return { blob, fileName };
        })
      );
  }
}

function parseFileName(disposition: string | null): string | null {
  if (!disposition) return null;
  const utf8Match = /filename\*=UTF-8''([^;]+)/i.exec(disposition);
  if (utf8Match) {
    try { return decodeURIComponent(utf8Match[1]); } catch { /* fall through */ }
  }
  const plain = /filename="?([^";]+)"?/i.exec(disposition);
  return plain ? plain[1] : null;
}
